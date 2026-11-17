using Fraud.Contracts;
using Fraud.Data.Entities;
using Fraud.Data.Repositories;
using Fraud.Scoring.Infrastructure;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring.Handling
{
    public class TransactionHandler
    {
        private readonly RiskScorer _riskScore;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICustomerStateStore _customerStateStore;
        private readonly CaseWriter _caseWriter;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;
        public TransactionHandler(RiskScorer riskScore, ITransactionRepository transactionRepository, ICustomerStateStore customerStateStore, CaseWriter caseWriter, TimeProvider timeProvider, IUnitOfWork unitOfWork)
        {
            _riskScore = riskScore;
            _transactionRepository = transactionRepository;
            _customerStateStore = customerStateStore;
            _caseWriter = caseWriter;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleTransaction(TransactionCreated tx, CancellationToken cancellationToken)
        {
            var IdempotencyCheck = await _transactionRepository.GetTransactionByIdAsync(tx.TransactionId);

            if (IdempotencyCheck is not null)
            {
                // Transaction has already been processed, skip further processing
                return;
            }

            CustomerState? customerState = await _customerStateStore.GetCustomerState(tx.CustomerId, cancellationToken);

            if (customerState is null)
            {
                customerState = CustomerState.EmptyState;
            }


            RiskAssessment riskAssessment = _riskScore.Assess(customerState, tx, tx.OccurredAt);

            var transactionEntity = await CreateTransaction(tx, riskAssessment);

            if (riskAssessment.ShouldReview)
            {
                await _caseWriter.WriteCaseAsync(transaction: transactionEntity, assessment: riskAssessment);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await UpdateState(tx, customerState, cancellationToken);
        }

        private async Task<TransactionEntity> CreateTransaction(TransactionCreated tx, RiskAssessment assessment)
        {
            return await _transactionRepository.AddTransactionAsync(new TransactionEntity()
            {
                Id = tx.TransactionId,
                CustomerId = tx.CustomerId,
                MerchantId = tx.MerchantId,
                MerchantCategory = tx.MerchantCategory,
                Decision = assessment.Decision,
                Channel = tx.Channel,
                Amount = tx.Amount,
                Country = tx.Country,
                Currency = tx.Currency,
                RiskScore = assessment.Score,
                OccurredAt = tx.OccurredAt,
                ScoredAt = _timeProvider.GetUtcNow()
            });
        }

        private async Task UpdateState(TransactionCreated tx, CustomerState State, CancellationToken cancellationToken)
        {

            CustomerState UpdatedState = CustomerState.EmptyState;

            if (State == CustomerState.EmptyState)
            {
                UpdatedState = new CustomerState(
                    recentTransactionTimes: new List<DateTimeOffset> { tx.OccurredAt },
                    lastCountry: tx.Country,
                    lastTransactionAt: tx.OccurredAt,
                    averageAmount: tx.Amount,
                    transactionCount: 1
                );

            }
            else
            {
                UpdatedState.RecentTransactionTimes = [.. UpdatedState.RecentTransactionTimes.Where(t => tx.OccurredAt - t <= TimeSpan.FromDays(30))];
                UpdatedState.LastCountry = tx.Country;
                UpdatedState.LastTransactionAt = tx.OccurredAt;
                UpdatedState.AverageAmount = ((UpdatedState.AverageAmount * UpdatedState.TransactionCount) + tx.Amount)
                                              / (UpdatedState.TransactionCount + 1);
                UpdatedState.TransactionCount += 1;
            }

            await _customerStateStore.UpsertCustomerState(tx.CustomerId, UpdatedState, cancellationToken);
        }
    }
}

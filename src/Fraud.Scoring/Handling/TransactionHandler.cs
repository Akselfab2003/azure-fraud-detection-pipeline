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
        private readonly RiskScore _riskScore;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICustomerStateStore _customerStateStore;
        private readonly CaseWriter _caseWriter;
        private readonly TimeProvider _timeProvider;

        public TransactionHandler(RiskScore riskScore, ITransactionRepository transactionRepository, ICustomerStateStore customerStateStore, CaseWriter caseWriter, TimeProvider timeProvider)
        {
            _riskScore = riskScore;
            _transactionRepository = transactionRepository;
            _customerStateStore = customerStateStore;
            _caseWriter = caseWriter;
            _timeProvider = timeProvider;
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
                customerState = new CustomerState([tx.OccurredAt], tx.Country, tx.OccurredAt, tx.Amount, 1);
            }

            RiskAssessment riskAssessment = _riskScore.Assess(customerState, tx, tx.OccurredAt);

            var transactionEntity = await CreateTransaction(tx, riskAssessment);

            if (riskAssessment.ShouldReview)
            {
                await _caseWriter.WriteCaseAsync(transaction: transactionEntity, assessment: riskAssessment);
            }

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
    }
}

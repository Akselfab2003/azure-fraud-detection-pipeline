using Fraud.Data.Entities;
using Fraud.Data.Repositories;
using Fraud.Scoring.Scoring;

namespace Fraud.Scoring.Infrastructure
{
    public class CaseWriter
    {
        private readonly ICaseRepository _caseRepository;
        private readonly TimeProvider _timeProvider;
        public CaseWriter(ICaseRepository caseRepository, TimeProvider timeProvider)
        {
            _caseRepository = caseRepository;
            _timeProvider = timeProvider;
        }

        public async Task WriteCaseAsync(TransactionEntity transaction, RiskAssessment assessment)
        {
            // Handles the case where the transaction is flagged for review
            await _caseRepository.AddCaseAsync(new CaseEntity()
            {
                Id = Guid.CreateVersion7(),
                RiskScore = assessment.Score,
                Transaction = transaction,
                AnomalyScore = null,
                Decision = assessment.Decision,
                Label = null,
                Reasons = [.. assessment.RuleHits.Select(r => new CaseReason(
                        r.ReasonCode,
                        r.Points,
                        r.Description))],
                RuleSetVersion = "1.0.0",
                TransactionId = transaction.Id,
                Status = Data.Enums.CaseStatus.Open,
                UpdatedAt = _timeProvider.GetUtcNow(),
                CreatedAt = _timeProvider.GetUtcNow()
            });
        }
    }
}

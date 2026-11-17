using Fraud.Data.Entities;
using Fraud.Data.Repositories;
using Fraud.Scoring.Scoring;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Infrastructure
{
    public class CaseWriter
    {
        private readonly ScoringOptions _options;
        private readonly ICaseRepository _caseRepository;
        private readonly TimeProvider _timeProvider;
        public CaseWriter(ICaseRepository caseRepository, TimeProvider timeProvider, IOptions<ScoringOptions> options)
        {
            _caseRepository = caseRepository;
            _timeProvider = timeProvider;
            _options = options.Value;
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
                RuleSetVersion = _options.RulesCheckVersion,
                TransactionId = transaction.Id,
                Status = Data.Enums.CaseStatus.Open,
                UpdatedAt = _timeProvider.GetUtcNow(),
                CreatedAt = _timeProvider.GetUtcNow()
            });
        }
    }
}

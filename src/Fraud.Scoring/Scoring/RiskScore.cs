using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring
{
    public class RiskScore
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly ScoringOptions _scoringOptions;

        public RiskScore(IEnumerable<IRule> rules, IOptions<ScoringOptions> scoringOptions)
        {
            _rules = rules;
            _scoringOptions = scoringOptions.Value;
        }

        public async Task<RiskAssessment> Assess(CustomerState c, TransactionCreated tx, DateTimeOffset now)
        {
            var hits = _rules.Select(r => r.Evaluate(tx, c, now))
                             .OfType<RuleHit>()
                             .ToList();

            var score = hits.Sum(h => h.Points);

            return new RiskAssessment()
            {
                Score = score,
                Decision = score >= _scoringOptions.CaseThreshold ? Decision.Review : Decision.Allow,
                RuleHits = hits,
            };
        }
    }
}

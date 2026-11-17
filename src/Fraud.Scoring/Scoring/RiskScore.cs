using Fraud.Contracts;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring
{
    public class RiskScore
    {
        private readonly ILogger<RiskScore> _logger;
        private readonly IEnumerable<IRule> _rules;
        private readonly ScoringOptions _scoringOptions;

        public RiskScore(ILogger<RiskScore> logger, IEnumerable<IRule> rules, IOptions<ScoringOptions> scoringOptions)
        {
            _logger = logger;
            _rules = rules;
            _scoringOptions = scoringOptions.Value;
        }

        public async Task<RiskAssessment> CalculateRiskScore(CustomerState c, TransactionCreated tx, DateTimeOffset now)
        {
            IEnumerable<RuleHit?> rulesResults = _rules.Select(r => r.Evaluate(tx, c, now));
            var hits = rulesResults.Where(r => r is not null).ToList();
            var score = hits.Sum(r => r!.Points);

            return new RiskAssessment()
            {
                Score = score,
                RuleHits = hits
            };
        }
    }
}

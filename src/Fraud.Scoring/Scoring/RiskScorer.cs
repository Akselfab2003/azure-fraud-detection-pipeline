using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring
{
    public class RiskScorer
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly ScoringOptions _scoringOptions;

        public RiskScorer(IEnumerable<IRule> rules, IOptions<ScoringOptions> scoringOptions)
        {
            _rules = rules;
            _scoringOptions = scoringOptions.Value;
        }

        public async Task<RiskAssessment> AssessAsync(CustomerState c, TransactionCreated tx, DateTimeOffset now)
        {
            // One rule at a time: rules may query the database, and the DbContext is not thread-safe.
            var hits = new List<RuleHit>();
            foreach (var rule in _rules)
            {
                if (await rule.Evaluate(tx, c, now) is { } hit)
                    hits.Add(hit);
            }

            var score = hits.Sum(h => h.Points);

            return new RiskAssessment()
            {
                Score = score,
                Decision = Decide(score),
                RuleHits = hits,
            };
        }

        // Block wins over Review; both thresholds are inclusive.
        private Decision Decide(int score) =>
            score >= _scoringOptions.BlockThreshold ? Decision.Block
            : score >= _scoringOptions.ReviewThreshold ? Decision.Review
            : Decision.Allow;
    }
}

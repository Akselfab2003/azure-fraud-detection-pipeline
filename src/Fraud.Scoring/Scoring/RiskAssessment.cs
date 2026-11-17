using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring.Scoring
{
    public sealed record RiskAssessment
    {
        public double Score { get; init; }
        public IReadOnlyList<RuleHit?> RuleHits { get; init; } = [];
    }
}

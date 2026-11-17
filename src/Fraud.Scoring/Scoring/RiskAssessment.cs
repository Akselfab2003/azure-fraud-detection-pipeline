using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring.Scoring
{
    public sealed record RiskAssessment
    {
        public required int Score { get; init; }
        public required Decision Decision { get; init; }
        public IReadOnlyList<RuleHit> RuleHits { get; init; } = [];
    }
}

using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class AmountBaselineRule(IOptions<ScoringOptions> _o) : IRule
    {
        private const string REASONCODE = "AMOUNT_BASELINE";
        private const string DESCRIPTION = "Transaction amount is significantly higher than the customer's baseline.";
        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            if (state.AverageAmount is Decimal.Zero)
            {
                // No baseline available, cannot evaluate this rule
                return null;
            }

            var threshold = state.AverageAmount * _o.Value.AmountMultiplier;

            return transaction.Amount > threshold
                ? new RuleHit(REASONCODE, _o.Value.AmountPoints, DESCRIPTION)
                : null;
        }
    }
}

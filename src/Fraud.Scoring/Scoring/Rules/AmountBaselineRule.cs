using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class AmountBaselineRule(IOptions<ScoringOptions> _o) : IRule
    {
        private const string REASONCODE = "AMOUNT_SPIKE";
        private const string DESCRIPTION = "Transaction amount is significantly higher than the customer's baseline.";
        public async Task<RuleHit?> Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            if (state.TransactionCount < _o.Value.MinHistory)
            {
                // Not enough history for a meaningful baseline (covers the first-ever transaction)
                return null;
            }

            var threshold = state.AverageAmount * _o.Value.AmountMultiplier;

            return transaction.Amount > threshold
                ? new RuleHit(REASONCODE, _o.Value.AmountPoints, DESCRIPTION)
                : null;
        }
    }
}

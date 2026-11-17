using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class ImpossibleTravelRule(IOptions<ScoringOptions> options) : IRule
    {
        private readonly ScoringOptions _options = options.Value;
        private const string REASONCODE = "IMPOSSIBLE_TRAVEL";
        private const string DESCRIPTION = "Transaction location is inconsistent with previous transactions.";
        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            if (state.LastCountry is null || state.LastTransactionAt is null)
            {
                // No previous transaction data, cannot evaluate this rule
                return null;
            }

            // Check if the transaction is from a different country and within the velocity window
            if (transaction.Country != state.LastCountry && (transaction.OccurredAt - state.LastTransactionAt.Value) < _options.VelocityWindow)
            {
                return new RuleHit(REASONCODE, 40, DESCRIPTION);
            }

            return null;
        }
    }
}

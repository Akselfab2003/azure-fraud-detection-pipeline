using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class ImpossibleTravelRule(IOptions<ScoringOptions> options) : IRule
    {
        private const string REASONCODE = "IMPOSSIBLE_TRAVEL";
        private const string DESCRIPTION = "Transaction location is inconsistent with previous transactions.";
        private readonly ScoringOptions _options = options.Value;

        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            if (state.LastCountry is null || state.LastTransactionAt is null)
            {
                // No previous transaction data, cannot evaluate this rule
                return null;
            }

            // A different country within the window is too fast to be a real trip.
            var sinceLast = now - state.LastTransactionAt.Value;

            return transaction.Country != state.LastCountry && sinceLast < _options.ImpossibleTravelWindow
                ? new RuleHit(REASONCODE, _options.ImpossibleTravelPoints, DESCRIPTION)
                : null;
        }
    }
}

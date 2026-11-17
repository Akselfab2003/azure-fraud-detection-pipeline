using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Scoring.Rules
{
    public class VelocityRule(IOptions<ScoringOptions> options) : IRule
    {
        private const string REASONCODE = "VELOCITY";
        private const string DESCRIPTION = "Too many transactions in a short period of time";
        private readonly ScoringOptions _o = options.Value;

        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            var inWindow = state.RecentTransactionTimes.Count(t => now - t < _o.VelocityWindow);
            var transactions = inWindow + 1; // +1 for the current transaction

            return transactions > _o.VelocityMaxTransactions
                ? new RuleHit(REASONCODE, _o.VelocityPoints, DESCRIPTION)
                : null;
        }
    }
}

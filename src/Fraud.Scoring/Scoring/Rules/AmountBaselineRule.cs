using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring.Scoring.Rules
{
    public class AmountBaselineRule : IRule
    {
        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now)
        {
            throw new NotImplementedException();
        }
    }
}

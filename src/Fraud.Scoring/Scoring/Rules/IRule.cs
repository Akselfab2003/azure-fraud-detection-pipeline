using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring.Scoring.Rules
{
    public interface IRule
    {
        RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now);
    }
}

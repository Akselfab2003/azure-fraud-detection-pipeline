using Fraud.Contracts;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring.Scoring.Rules
{
    public interface IRule
    {
        /// <summary>
        /// Evaluates the rule against the given transaction and customer state.
        /// </summary>
        /// <param name="transaction">The transaction to evaluate.</param>
        /// <param name="state">The customer state to evaluate against.</param>
        /// <param name="now">The current time.</param>
        /// <returns>The result of the evaluation, or null if the rule does not apply.</returns>
        RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now);
    }
}

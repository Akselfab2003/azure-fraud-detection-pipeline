using Fraud.Contracts;
using Fraud.Simulator.Patterns;

namespace Fraud.Simulator.Generator
{
    public sealed record GeneratedTransaction(TransactionCreated Message, FraudPattern Pattern)
    {
        public bool IsFraud => Pattern != FraudPattern.None;
    }
}

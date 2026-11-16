using Fraud.Contracts;
using Fraud.Simulator.DataModels;

namespace Fraud.Simulator.Generator
{
    public sealed record GeneratedTransaction(TransactionCreated Message, FraudPattern Pattern)
    {
        public bool IsFraud => Pattern != FraudPattern.None;
    }
}

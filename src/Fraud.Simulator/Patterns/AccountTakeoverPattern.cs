using Fraud.Simulator.DataModels;
using Fraud.Simulator.Generator;

namespace Fraud.Simulator.Patterns
{
    public sealed class AccountTakeoverPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.AccountTakeover;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, IReadOnlyList<Merchant> merchants, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

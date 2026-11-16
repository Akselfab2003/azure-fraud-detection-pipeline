using Fraud.Simulator.Generator;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public sealed class NewMerchantBurstPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.NewMerchantBurst;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, IReadOnlyList<Merchant> merchants, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

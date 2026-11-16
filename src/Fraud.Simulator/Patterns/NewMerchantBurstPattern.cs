using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public sealed class NewMerchantBurstPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.NewMerchantBurst;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

using Fraud.Simulator.Generator;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public sealed class ImpossibleTravelPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.ImpossibleTravel;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, IReadOnlyList<Merchant> merchants, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

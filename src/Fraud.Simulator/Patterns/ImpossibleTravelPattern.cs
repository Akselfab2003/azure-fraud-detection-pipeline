using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public sealed class ImpossibleTravelPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.ImpossibleTravel;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

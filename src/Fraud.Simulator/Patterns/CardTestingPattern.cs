using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public sealed class CardTestingPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.CardTesting;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            throw new NotImplementedException();
        }
    }
}

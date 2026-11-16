using Fraud.Simulator.Generator;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    public interface IFraudPattern
    {
        FraudPattern Kind { get; }

        IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, IReadOnlyList<Merchant> merchants, DateTimeOffset start, Random random);
    }
}

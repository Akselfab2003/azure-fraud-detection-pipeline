using Fraud.Simulator.DataModels;
using Fraud.Simulator.Generator;

namespace Fraud.Simulator.Patterns
{
    public interface IFraudPattern
    {
        FraudPattern Kind { get; }

        IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, IReadOnlyList<Merchant> merchants, DateTimeOffset start, Random random);
    }
}

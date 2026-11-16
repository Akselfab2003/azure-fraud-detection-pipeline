using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    /// <summary>
    /// A run of purchases at several merchants the customer has never used, in a short window.
    /// Amounts look ordinary; the signal is velocity plus novelty.
    /// </summary>
    public sealed class NewMerchantBurstPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.NewMerchantBurst;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            var newMerchants = PatternSupport.NewMerchants(customer, population);
            var local = newMerchants.Where(m => m.Country == customer.HomeCountry).ToList();
            var pool = (local.Count >= 4 ? local : newMerchants).ToArray();

            var count = Math.Min(random.Next(4, 9), pool.Length);  // 4-8 merchants
            if (count < 2)
                return [];

            random.Shuffle(pool);

            var result = new List<GeneratedTransaction>(count);
            var at = start;
            for (var i = 0; i < count; i++)
            {
                at = PatternSupport.Advance(at, random, 30, 180);  // 30s-3min apart
                var merchant = pool[i];
                result.Add(PatternSupport.Create(customer, merchant, PatternSupport.NormalAmount(customer, merchant, random),
                                                 PatternSupport.PickChannel(merchant, Channel.CardPresent), at, Kind));
            }

            return result;
        }
    }
}

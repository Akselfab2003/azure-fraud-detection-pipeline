using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    /// <summary>
    /// Someone else controls the account: a few online purchases at new merchants
    /// in high-value categories, each several times the customer's typical amount.
    /// </summary>
    public sealed class AccountTakeoverPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.AccountTakeover;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            var newMerchants = PatternSupport.NewMerchants(customer, population);
            var targets = newMerchants.Where(m => MerchantCatalog.Get(m.Category).FraudTarget).ToList();
            var candidates = targets.Count > 0 ? targets : newMerchants;
            if (candidates.Count == 0)
                return [];

            var count = random.Next(2, 5);                        // 2-4 purchases
            var result = new List<GeneratedTransaction>(count);
            var at = start;

            for (var i = 0; i < count; i++)
            {
                at = PatternSupport.Advance(at, random, 60, 1200); // 1-20 minutes apart
                var merchant = PatternSupport.PickOne(candidates, random);
                var amount = Math.Round(customer.TypicalAmount * (decimal)(3 + random.NextDouble() * 5), 2);
                result.Add(PatternSupport.Create(customer, merchant, amount,
                                                 PatternSupport.PickChannel(merchant, Channel.Online), at, Kind));
            }

            return result;
        }
    }
}

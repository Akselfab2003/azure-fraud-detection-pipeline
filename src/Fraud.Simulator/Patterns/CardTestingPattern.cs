using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    /// <summary>
    /// A stolen card is probed with many tiny online purchases at one merchant,
    /// followed by one larger purchase once the card is known to work.
    /// </summary>
    public sealed class CardTestingPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.CardTesting;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            var online = PatternSupport.NewMerchants(customer, population)
                                       .Where(m => PatternSupport.SupportsChannel(m, Channel.Online))
                                       .ToList();
            var targets = online.Where(m => MerchantCatalog.Get(m.Category).FraudTarget).ToList();
            var candidates = targets.Count > 0 ? targets : online;
            if (candidates.Count == 0)
                return [];

            var merchant = PatternSupport.PickOne(candidates, random);
            var tests = random.Next(5, 13);                       // 5-12 probes
            var result = new List<GeneratedTransaction>(tests + 1);
            var at = start;

            for (var i = 0; i < tests; i++)
            {
                at = PatternSupport.Advance(at, random, 5, 40);   // seconds between probes
                result.Add(PatternSupport.Create(customer, merchant, PatternSupport.Money(1 + random.NextDouble() * 9),
                                                 Channel.Online, at, Kind));
            }

            at = PatternSupport.Advance(at, random, 30, 120);     // cash-out
            var cashOut = customer.TypicalAmount * (decimal)(1.5 + random.NextDouble() * 2);
            result.Add(PatternSupport.Create(customer, merchant, Math.Round(cashOut, 2), Channel.Online, at, Kind));
            return result;
        }
    }
}

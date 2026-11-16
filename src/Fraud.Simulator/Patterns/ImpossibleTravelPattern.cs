using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    /// <summary>
    /// A legitimate card-present purchase at home (the unlabelled "anchor"), then 10-45 minutes later
    /// card-present purchases in another country. Only the foreign purchases are labelled fraud.
    /// </summary>
    public sealed class ImpossibleTravelPattern : IFraudPattern
    {
        public FraudPattern Kind => FraudPattern.ImpossibleTravel;

        public IReadOnlyList<GeneratedTransaction> Generate(CustomerProfile customer, Population.Population population, DateTimeOffset start, Random random)
        {
            if (customer.UsualMerchantIds.Count == 0)
                return [];

            var anchorMerchant = population.MerchantsById[PatternSupport.PickOne(customer.UsualMerchantIds, random)];

            var foreign = population.Merchants
                                    .Where(m => m.Country != customer.HomeCountry && m.Country != anchorMerchant.Country)
                                    .ToList();
            if (foreign.Count == 0)
                return [];

            var country = PatternSupport.PickOne(foreign, random).Country;
            var abroad = foreign.Where(m => m.Country == country).ToList();

            var result = new List<GeneratedTransaction>
            {
                PatternSupport.Create(customer, anchorMerchant,
                                      PatternSupport.NormalAmount(customer, anchorMerchant, random),
                                      PatternSupport.PickChannel(anchorMerchant, Channel.CardPresent),
                                      start, FraudPattern.None),
            };

            var at = start.AddMinutes(random.Next(10, 46));
            var count = random.Next(1, 4);                          // 1-3 foreign purchases
            for (var i = 0; i < count; i++)
            {
                if (i > 0)
                    at = PatternSupport.Advance(at, random, 60, 600);

                var merchant = PatternSupport.PickOne(abroad, random);
                result.Add(PatternSupport.Create(customer, merchant,
                                                 PatternSupport.NormalAmount(customer, merchant, random),
                                                 PatternSupport.PickChannel(merchant, Channel.CardPresent),
                                                 at, Kind));
            }

            return result;
        }
    }
}

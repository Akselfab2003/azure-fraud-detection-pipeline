using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Patterns
{
    /// <summary>Small helpers shared by the fraud patterns.</summary>
    internal static class PatternSupport
    {
        public static GeneratedTransaction Create(CustomerProfile customer, Merchant merchant, decimal amount,
                                                  Channel channel, DateTimeOffset at, FraudPattern pattern)
            => new(new TransactionCreated(
                       TransactionId: Guid.CreateVersion7(),
                       CustomerId: customer.Id,
                       MerchantId: merchant.Id,
                       MerchantCategory: merchant.Category,
                       Amount: amount,
                       Currency: "DKK",
                       Country: merchant.Country,
                       Channel: channel,
                       OccurredAt: at),
                   pattern);

        public static decimal Money(double value) => Math.Round((decimal)value, 2);

        /// <summary>An amount in line with the customer's normal spending at this kind of merchant.</summary>
        public static decimal NormalAmount(CustomerProfile customer, Merchant merchant, Random random)
            => Math.Round(customer.TypicalAmount
                          * (decimal)MerchantCatalog.Get(merchant.Category).AmountMultiplier
                          * (decimal)RandomExtensions.LogNormal(random, 1.0, (double)customer.AmountSpread), 2);

        /// <summary>Merchants the customer has never used.</summary>
        public static List<Merchant> NewMerchants(CustomerProfile customer, Population.Population population)
        {
            var usual = customer.UsualMerchantIds.ToHashSet();
            return population.Merchants.Where(m => !usual.Contains(m.Id)).ToList();
        }

        public static bool SupportsChannel(Merchant merchant, Channel channel)
            => MerchantCatalog.Get(merchant.Category).Channels.Contains(channel);

        /// <summary>The preferred channel if the merchant's category supports it, otherwise its main channel.</summary>
        public static Channel PickChannel(Merchant merchant, Channel preferred)
        {
            var channels = MerchantCatalog.Get(merchant.Category).Channels;
            return channels.Contains(preferred) ? preferred : channels[0];
        }

        public static DateTimeOffset Advance(DateTimeOffset from, Random random, int minSeconds, int maxSeconds)
            => from.AddSeconds(random.Next(minSeconds, maxSeconds + 1));

        public static T PickOne<T>(IReadOnlyList<T> items, Random random) => items[random.Next(items.Count)];
    }
}

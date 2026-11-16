using Fraud.Contracts;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Generation
{
    public sealed class NormalTraffic(Population.Population population, ActivityCurve activityCurve)
    {
        // Thinning: draw exponential gaps at the constant *peak* rate, keep a candidate at time t
        // with probability weight(t) / maxWeight. The kept events have rate TransactionsPerDay * weight(t).
        public DateTimeOffset NextTime(CustomerProfile c, DateTimeOffset after, Random rng)
        {
            var peakRatePerDay = c.TransactionsPerDay * activityCurve.MaxWeight;
            var t = after;
            while (true)
            {
                // 1 - NextDouble() is in (0, 1], so the log is always finite.
                t += TimeSpan.FromDays(-Math.Log(1.0 - rng.NextDouble()) / peakRatePerDay);
                if (rng.NextDouble() < activityCurve.Weight(t) / activityCurve.MaxWeight)
                    return t;
            }
        }

        public GeneratedTransaction Create(CustomerProfile c, DateTimeOffset at, Random rng)
        {
            var merchant = rng.NextDouble() < 0.9
                ? population.MerchantsById[c.UsualMerchantIds[rng.Next(c.UsualMerchantIds.Count)]]
                : population.Merchants[rng.Next(population.Merchants.Count)];

            var cat = MerchantCatalog.Get(merchant.Category);
            var amount = Math.Round(c.TypicalAmount * (decimal)cat.AmountMultiplier * (decimal)RandomExtensions.LogNormal(rng, median: 1.0, sigma: (double)c.AmountSpread), 2);
            return new GeneratedTransaction(
                new TransactionCreated
                (
                    TransactionId: Guid.CreateVersion7(),
                    CustomerId: c.Id,
                    MerchantId: merchant.Id,
                    MerchantCategory: merchant.Category,
                    Amount: amount,
                    Currency: "DKK",
                    Country: merchant.Country,
                    Channel: cat.Channels[0],
                    OccurredAt: at
                ),
                Pattern: Patterns.FraudPattern.None);
        }

    }
}

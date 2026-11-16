using Fraud.Contracts;
using Fraud.Simulator.Generator;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Generation
{
    public sealed class NormalTraffic(Population.Population population, ActivityCurve activityCurve)
    {
        // Thinning: draw exponential gaps at the *peak* rate, accept with weight/peakWeight.
        public DateTimeOffset NextTime(CustomerProfile c, DateTimeOffset after, Random rng)
        {
            var peakRate = c.TransactionsPerDay * activityCurve.Weight(after);
            var gap = TimeSpan.FromDays(-Math.Log(rng.NextDouble()) / peakRate);
            var next = after + gap;
            var weight = activityCurve.Weight(next);
            if (rng.NextDouble() < weight)
                return next;
            else
                return NextTime(c, next, rng);
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

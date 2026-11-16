using Fraud.Simulator.Configuration;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Population;
using PopulationModel = Fraud.Simulator.Population.Population;

namespace Fraud.Simulator.Tests.Generation
{
    public class NormalTrafficTests
    {
        private static readonly DateTimeOffset Start = new(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);

        private static (NormalTraffic Traffic, PopulationModel Population) Create()
        {
            var population = new PopulationBuilder(new SimulatorOptions { Seed = 3, CustomerCount = 50, MerchantCount = 100 }).Build();
            return (new NormalTraffic(population, new ActivityCurve()), population);
        }

        private static CustomerProfile Customer(double perDay)
            => new(Guid.NewGuid(), "DK", 250m, 0.4m, perDay, []);

        [Fact]
        public void NextTime_ShouldAlwaysBeAfterTheGivenTime()
        {
            var (traffic, _) = Create();
            var rng = new Random(1);
            var t = Start;

            for (var i = 0; i < 1000; i++)
            {
                var next = traffic.NextTime(Customer(3), t, rng);
                Assert.True(next > t);
                t = next;
            }
        }

        [Fact]
        public void NextTime_ShouldAverageTransactionsPerDayOverLongRuns()
        {
            var (traffic, _) = Create();
            var customer = Customer(3);
            var rng = new Random(2);
            const int days = 400;
            var end = Start.AddDays(days);

            var count = 0;
            for (var t = traffic.NextTime(customer, Start, rng); t < end; t = traffic.NextTime(customer, t, rng))
                count++;

            Assert.InRange(count / (double)days, 2.7, 3.3);
        }

        [Fact]
        public void NextTime_ShouldBeBusierInTheEveningThanAtNight()
        {
            var (traffic, _) = Create();
            var customer = Customer(5);
            var rng = new Random(4);
            var end = Start.AddDays(300);

            var night = 0;
            var evening = 0;
            for (var t = traffic.NextTime(customer, Start, rng); t < end; t = traffic.NextTime(customer, t, rng))
            {
                if (t.Hour is >= 1 and <= 4) night++;
                if (t.Hour is >= 17 and <= 19) evening++;
            }

            // Per hour: 4 night hours vs 3 evening hours; expected ratio is roughly 18x.
            Assert.True(evening / 3.0 > 5 * (night / 4.0), $"evening={evening}, night={night}");
        }

        [Fact]
        public void Create_ShouldProduceAnUnlabelledTransactionForTheCustomer()
        {
            var (traffic, population) = Create();
            var customer = population.Customers[0];

            var tx = traffic.Create(customer, Start, new Random(5));

            Assert.False(tx.IsFraud);
            Assert.Equal(customer.Id, tx.Message.CustomerId);
            Assert.Equal(Start, tx.Message.OccurredAt);
            Assert.True(tx.Message.Amount > 0);
            Assert.Contains(tx.Message.MerchantId, population.MerchantsById.Keys);
            Assert.Equal(population.MerchantsById[tx.Message.MerchantId].Category, tx.Message.MerchantCategory);
        }
    }
}

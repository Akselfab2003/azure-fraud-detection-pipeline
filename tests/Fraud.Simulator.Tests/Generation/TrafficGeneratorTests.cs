using Fraud.Simulator.Configuration;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Patterns;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Tests.Generation
{
    public class TrafficGeneratorTests
    {
        private static readonly DateTimeOffset Start = new(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);

        private static TrafficGenerator Create(SimulatorOptions options)
        {
            var population = new PopulationBuilder(options).Build();
            var normal = new NormalTraffic(population, new ActivityCurve());
            IFraudPattern[] patterns =
            [
                new CardTestingPattern(),
                new AccountTakeoverPattern(),
                new ImpossibleTravelPattern(),
                new NewMerchantBurstPattern(),
            ];
            return new TrafficGenerator(population, normal, patterns, options, new FixedTimeProvider(Start));
        }

        private static List<GeneratedTransaction> Take(SimulatorOptions options, int count)
            => Create(options).Stream().Take(count).ToList();

        [Fact]
        public void Stream_SameSeed_ShouldGiveSameTransactions()
        {
            // TransactionId is a time-ordered v7 Guid and not seeded, so it is left out of the comparison.
            static IEnumerable<object> Shape(List<GeneratedTransaction> txs)
                => txs.Select(t => (object)(t.Message.CustomerId, t.Message.MerchantId, t.Message.Amount, t.Message.OccurredAt, t.Pattern));

            var a = Take(new SimulatorOptions { Seed = 9 }, 2000);
            var b = Take(new SimulatorOptions { Seed = 9 }, 2000);

            Assert.Equal(Shape(a), Shape(b));
        }

        [Fact]
        public void Stream_ShouldBeOrderedByTime()
        {
            var txs = Take(new SimulatorOptions(), 20_000);

            for (var i = 1; i < txs.Count; i++)
                Assert.True(txs[i].Message.OccurredAt >= txs[i - 1].Message.OccurredAt, $"out of order at {i}");
        }

        [Fact]
        public void Stream_ShouldStartAtOrAfterTheClock()
        {
            var txs = Take(new SimulatorOptions(), 500);

            Assert.All(txs, t => Assert.True(t.Message.OccurredAt >= Start));
        }

        [Fact]
        public void Stream_FraudShare_ShouldMatchTheConfiguredShare()
        {
            const int count = 30_000;
            var txs = Take(new SimulatorOptions { FraudShare = 0.02 }, count);

            var share = txs.Count(t => t.IsFraud) / (double)count;

            Assert.InRange(share, 0.015, 0.025);
        }

        [Fact]
        public void Stream_ZeroFraudShare_ShouldProduceNoFraud()
        {
            var txs = Take(new SimulatorOptions { FraudShare = 0 }, 5000);

            Assert.DoesNotContain(txs, t => t.IsFraud);
        }

        [Fact]
        public void Stream_ShouldOnlyInjectEnabledPatterns()
        {
            var txs = Take(new SimulatorOptions { FraudShare = 0.05, EnabledPatterns = [FraudPattern.CardTesting] }, 10_000);

            var fraud = txs.Where(t => t.IsFraud).ToList();
            Assert.NotEmpty(fraud);
            Assert.All(fraud, t => Assert.Equal(FraudPattern.CardTesting, t.Pattern));
        }

        [Fact]
        public void Stream_NoEnabledPatterns_ShouldProduceNoFraud()
        {
            var txs = Take(new SimulatorOptions { EnabledPatterns = [] }, 5000);

            Assert.DoesNotContain(txs, t => t.IsFraud);
        }

        [Fact]
        public void Stream_WithAllPatternsEnabled_ShouldInjectEachOfThem()
        {
            var txs = Take(new SimulatorOptions { FraudShare = 0.05 }, 30_000);

            var seen = txs.Where(t => t.IsFraud).Select(t => t.Pattern).ToHashSet();
            Assert.Equal(4, seen.Count);
        }

        [Fact]
        public void Stream_ShouldHaveFarFewerTransactionsAtNightThanInTheEvening()
        {
            var txs = Take(new SimulatorOptions { FraudShare = 0 }, 50_000);

            var night = txs.Count(t => t.Message.OccurredAt.Hour is >= 1 and <= 4);
            var evening = txs.Count(t => t.Message.OccurredAt.Hour is >= 17 and <= 19);

            Assert.True(evening / 3.0 > 5 * (night / 4.0), $"evening={evening}, night={night}");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.0)]
        [InlineData(1.5)]
        public void Constructor_InvalidFraudShare_ShouldThrow(double share)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(new SimulatorOptions { FraudShare = share }));
        }
    }
}

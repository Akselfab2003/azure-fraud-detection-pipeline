using Fraud.Contracts.Enums;
using Fraud.Simulator.Configuration;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Patterns;
using Fraud.Simulator.Population;
using PopulationModel = Fraud.Simulator.Population.Population;

namespace Fraud.Simulator.Tests.Patterns
{
    public class FraudPatternTests
    {
        private static readonly DateTimeOffset Start = new(2026, 1, 5, 12, 0, 0, TimeSpan.Zero);

        private static readonly PopulationModel Population =
            new PopulationBuilder(new SimulatorOptions { Seed = 1, CustomerCount = 100, MerchantCount = 200 }).Build();

        private static IFraudPattern Create(FraudPattern kind) => kind switch
        {
            FraudPattern.CardTesting => new CardTestingPattern(),
            FraudPattern.AccountTakeover => new AccountTakeoverPattern(),
            FraudPattern.ImpossibleTravel => new ImpossibleTravelPattern(),
            FraudPattern.NewMerchantBurst => new NewMerchantBurstPattern(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        private static IEnumerable<(CustomerProfile Customer, IReadOnlyList<GeneratedTransaction> Burst)> Bursts(FraudPattern kind)
        {
            var pattern = Create(kind);
            foreach (var customer in Population.Customers)
                yield return (customer, pattern.Generate(customer, Population, Start, new Random(customer.Id.GetHashCode())));
        }

        private static HashSet<Guid> Usual(CustomerProfile c) => c.UsualMerchantIds.ToHashSet();

        // ---- properties every pattern must satisfy ----

        [Theory]
        [InlineData(FraudPattern.CardTesting)]
        [InlineData(FraudPattern.AccountTakeover)]
        [InlineData(FraudPattern.ImpossibleTravel)]
        [InlineData(FraudPattern.NewMerchantBurst)]
        public void Generate_ShouldProduceLabelledTimeOrderedTransactionsForTheCustomer(FraudPattern kind)
        {
            foreach (var (customer, burst) in Bursts(kind))
            {
                Assert.NotEmpty(burst);
                Assert.Contains(burst, t => t.IsFraud);
                Assert.All(burst, t =>
                {
                    Assert.Equal(customer.Id, t.Message.CustomerId);
                    Assert.True(t.Message.OccurredAt >= Start, "must not start before the requested start");
                    Assert.True(t.Message.Amount > 0);
                });
                Assert.All(burst.Where(t => t.IsFraud), t => Assert.Equal(kind, t.Pattern));

                var times = burst.Select(t => t.Message.OccurredAt).ToList();
                Assert.Equal(times.Order(), times);
            }
        }

        [Theory]
        [InlineData(FraudPattern.CardTesting)]
        [InlineData(FraudPattern.AccountTakeover)]
        [InlineData(FraudPattern.ImpossibleTravel)]
        [InlineData(FraudPattern.NewMerchantBurst)]
        public void Generate_SameRandomSeed_ShouldGiveSameShape(FraudPattern kind)
        {
            var pattern = Create(kind);
            var customer = Population.Customers[0];

            static IEnumerable<object> Shape(IReadOnlyList<GeneratedTransaction> burst)
                => burst.Select(t => (object)(t.Message.MerchantId, t.Message.Amount, t.Message.OccurredAt, t.Pattern));

            var a = pattern.Generate(customer, Population, Start, new Random(11));
            var b = pattern.Generate(customer, Population, Start, new Random(11));

            Assert.Equal(Shape(a), Shape(b));
        }

        // ---- pattern-specific shape ----

        [Fact]
        public void CardTesting_ShouldProbeOneNewMerchantWithTinyOnlineAmountsThenCashOut()
        {
            foreach (var (customer, burst) in Bursts(FraudPattern.CardTesting))
            {
                Assert.InRange(burst.Count, 6, 13);
                Assert.Single(burst.Select(t => t.Message.MerchantId).Distinct());
                Assert.DoesNotContain(burst[0].Message.MerchantId, Usual(customer));
                Assert.All(burst, t => Assert.Equal(Channel.Online, t.Message.Channel));

                Assert.All(burst.Take(burst.Count - 1), t => Assert.InRange(t.Message.Amount, 1m, 10m));
                Assert.True(burst[^1].Message.Amount > burst[^2].Message.Amount, "cash-out should exceed the probes");

                var span = burst[^1].Message.OccurredAt - burst[0].Message.OccurredAt;
                Assert.True(span < TimeSpan.FromMinutes(11), $"span was {span}");
            }
        }

        [Fact]
        public void AccountTakeover_ShouldBuyFewLargeOnlineItemsAtNewMerchants()
        {
            foreach (var (customer, burst) in Bursts(FraudPattern.AccountTakeover))
            {
                Assert.InRange(burst.Count, 2, 4);
                Assert.All(burst, t =>
                {
                    Assert.DoesNotContain(t.Message.MerchantId, Usual(customer));
                    Assert.True(t.Message.Amount >= customer.TypicalAmount * 3m, "amounts should be at least 3x typical");
                });
            }
        }

        [Fact]
        public void NewMerchantBurst_ShouldHitSeveralDistinctNewMerchantsQuickly()
        {
            foreach (var (customer, burst) in Bursts(FraudPattern.NewMerchantBurst))
            {
                Assert.InRange(burst.Count, 4, 8);
                Assert.Equal(burst.Count, burst.Select(t => t.Message.MerchantId).Distinct().Count());
                Assert.All(burst, t => Assert.DoesNotContain(t.Message.MerchantId, Usual(customer)));

                var span = burst[^1].Message.OccurredAt - burst[0].Message.OccurredAt;
                Assert.True(span <= TimeSpan.FromMinutes(30), $"span was {span}");
            }
        }

        [Fact]
        public void ImpossibleTravel_ShouldPairAHomePurchaseWithForeignPurchasesSoonAfter()
        {
            foreach (var (customer, burst) in Bursts(FraudPattern.ImpossibleTravel))
            {
                var anchor = burst[0];
                var foreign = burst.Skip(1).ToList();

                Assert.False(anchor.IsFraud, "the anchor is a genuine purchase");
                Assert.Equal(Start, anchor.Message.OccurredAt);
                Assert.Contains(anchor.Message.MerchantId, Usual(customer));

                Assert.InRange(foreign.Count, 1, 3);
                Assert.All(foreign, t =>
                {
                    Assert.True(t.IsFraud);
                    Assert.NotEqual(customer.HomeCountry, t.Message.Country);
                    Assert.NotEqual(anchor.Message.Country, t.Message.Country);
                });
                Assert.Single(foreign.Select(t => t.Message.Country).Distinct());

                var gap = foreign[0].Message.OccurredAt - anchor.Message.OccurredAt;
                Assert.InRange(gap.TotalMinutes, 10, 45);
            }
        }
    }
}

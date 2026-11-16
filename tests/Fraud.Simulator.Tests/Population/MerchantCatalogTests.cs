using Fraud.Simulator.Population;

namespace Fraud.Simulator.Tests.Population
{
    public class MerchantCatalogTests
    {
        [Fact]
        public void Categories_TotalWeight_ShouldBeOne()
        {
            var totalWeight = MerchantCatalog.Categories.Sum(c => c.Weight);

            Assert.Equal(1.0, totalWeight, precision: 9);
        }

        [Fact]
        public void Categories_Names_ShouldBeUnique()
        {
            var names = MerchantCatalog.Categories.Select(c => c.Name).ToList();

            Assert.Equal(names.Count, names.Distinct().Count());
        }

        [Fact]
        public void Categories_ShouldHaveSaneValues()
        {
            foreach (var c in MerchantCatalog.Categories)
            {
                Assert.True(c.Weight > 0, $"{c.Name}: weight must be positive");
                Assert.True(c.AmountMultiplier > 0, $"{c.Name}: amount multiplier must be positive");
                Assert.NotEmpty(c.Channels);
            }
        }

        [Fact]
        public void PickCategory_ShouldFollowWeights()
        {
            const int draws = 100_000;
            var rng = new Random(42);

            var shares = Enumerable.Range(0, draws)
                .Select(_ => MerchantCatalog.PickCategory(rng))
                .GroupBy(name => name)
                .ToDictionary(g => g.Key, g => g.Count() / (double)draws);

            foreach (var c in MerchantCatalog.Categories)
            {
                var actual = shares.GetValueOrDefault(c.Name);

                // Statistical tolerance: standard error at 100k draws is at most ~0.0016.
                Assert.InRange(actual, c.Weight - 0.01, c.Weight + 0.01);
            }
        }
    }
}

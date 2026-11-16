using Fraud.Simulator.Configuration;
using Fraud.Simulator.Population;
using PopulationModel = Fraud.Simulator.Population.Population;

namespace Fraud.Simulator.Tests.Population
{
    public class PopulationBuilderTests
    {
        private static PopulationModel Build(int seed = 42, int? customers = null, int? merchants = null)
        {
            var options = new SimulatorOptions { Seed = seed };
            if (customers is not null) options.CustomerCount = customers.Value;
            if (merchants is not null) options.MerchantCount = merchants.Value;
            return new PopulationBuilder(options).Build();
        }

        [Fact]
        public void BuildPopulation_ShouldReturnPopulationWithExpectedCounts()
        {
            var population = Build();

            var defaults = new SimulatorOptions();
            Assert.NotNull(population);
            Assert.Equal(defaults.CustomerCount, population.Customers.Count);
            Assert.Equal(defaults.MerchantCount, population.Merchants.Count);
        }

        [Fact]
        public void Build_SameSeed_ShouldGiveSameCustomersAndMerchants()
        {
            var a = Build(seed: 7);
            var b = Build(seed: 7);

            Assert.Equal(
                a.Merchants.Select(m => (m.Id, m.Category, m.Country)),
                b.Merchants.Select(m => (m.Id, m.Category, m.Country)));

            Assert.Equal(
                a.Customers.Select(c => (c.Id, c.HomeCountry, c.TypicalAmount, c.AmountSpread, c.TransactionsPerDay)),
                b.Customers.Select(c => (c.Id, c.HomeCountry, c.TypicalAmount, c.AmountSpread, c.TransactionsPerDay)));

            for (var i = 0; i < a.Customers.Count; i++)
                Assert.Equal(a.Customers[i].UsualMerchantIds, b.Customers[i].UsualMerchantIds);
        }

        [Fact]
        public void Build_DifferentSeed_ShouldGiveDifferentCustomers()
        {
            var a = Build(seed: 1);
            var b = Build(seed: 2);

            Assert.NotEqual(a.Customers.Select(c => c.Id), b.Customers.Select(c => c.Id));
        }

        [Fact]
        public void Build_RaisingCustomerCount_ShouldNotChangeExistingCustomers()
        {
            var small = Build(customers: 100);
            var large = Build(customers: 300);

            for (var i = 0; i < small.Customers.Count; i++)
            {
                Assert.Equal(small.Customers[i].Id, large.Customers[i].Id);
                Assert.Equal(small.Customers[i].TypicalAmount, large.Customers[i].TypicalAmount);
                Assert.Equal(small.Customers[i].UsualMerchantIds, large.Customers[i].UsualMerchantIds);
            }
        }

        [Fact]
        public void Build_Ids_ShouldBeUnique()
        {
            var population = Build();

            Assert.Equal(population.Customers.Count, population.Customers.Select(c => c.Id).Distinct().Count());
            Assert.Equal(population.Merchants.Count, population.Merchants.Select(m => m.Id).Distinct().Count());
        }

        [Fact]
        public void Build_Merchants_ShouldUseKnownCategories()
        {
            var population = Build();
            var known = MerchantCatalog.Categories.Select(c => c.Name).ToHashSet();

            Assert.All(population.Merchants, m => Assert.Contains(m.Category, known));
        }

        [Fact]
        public void Build_Customers_ShouldHaveValuesInExpectedRanges()
        {
            var population = Build();

            Assert.All(population.Customers, c =>
            {
                Assert.True(c.TypicalAmount > 0, "typical amount must be positive");
                Assert.InRange(c.AmountSpread, 0.3m, 0.7m);
                Assert.InRange(c.TransactionsPerDay, 0.2, 8.0);
                Assert.InRange(c.UsualMerchantIds.Count, 5, 15);
            });
        }

        [Fact]
        public void Build_UsualMerchants_ShouldExistAndBeDistinct()
        {
            var population = Build();
            var merchantIds = population.Merchants.Select(m => m.Id).ToHashSet();

            Assert.All(population.Customers, c =>
            {
                Assert.All(c.UsualMerchantIds, id => Assert.Contains(id, merchantIds));
                Assert.Equal(c.UsualMerchantIds.Count, c.UsualMerchantIds.Distinct().Count());
            });
        }

        [Fact]
        public void Build_UsualMerchants_ShouldBeInHomeCountryWhenEnoughExist()
        {
            var population = Build();
            var countryById = population.Merchants.ToDictionary(m => m.Id, m => m.Country);

            // DK has ~85% of 200 merchants, so every DK customer has plenty of local merchants.
            var danish = population.Customers.Where(c => c.HomeCountry == "DK").ToList();
            Assert.NotEmpty(danish);
            Assert.All(danish, c =>
                Assert.All(c.UsualMerchantIds, id => Assert.Equal("DK", countryById[id])));
        }

        [Fact]
        public void Build_HomeCountries_ShouldBeKnownAndMostlyDanish()
        {
            var population = Build();
            var allowed = new[] { "DK", "SE", "NO", "DE" };

            Assert.All(population.Customers, c => Assert.Contains(c.HomeCountry, allowed));

            var danishShare = population.Customers.Count(c => c.HomeCountry == "DK") / (double)population.Customers.Count;
            Assert.InRange(danishShare, 0.80, 0.90);
        }
    }
}

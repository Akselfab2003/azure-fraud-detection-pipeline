using Fraud.Simulator.Configuration;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Tests.Population
{
    public class PopulationBuilderTests
    {
        [Fact]
        public void BuildPopulation_ShouldReturnPopulationWithExpectedCounts()
        {
            // Arrange
            var populationBuilder = new PopulationBuilder(new() { Seed = 42 });

            // Act
            var population = populationBuilder.Build();

            // Assert
            SimulatorOptions default_options = new();
            Assert.NotNull(population);
            Assert.Equal(default_options.CustomerCount, population.Customers.Count);
            Assert.Equal(default_options.MerchantCount, population.Merchants.Count);
        }


    }
}

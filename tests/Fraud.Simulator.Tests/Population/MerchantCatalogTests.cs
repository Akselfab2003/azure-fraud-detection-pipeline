using Fraud.Simulator.Population;

namespace Fraud.Simulator.Tests.Population
{
    public class MerchantCatalogTests
    {
        [Fact]
        public async Task Categories_TotalWeight_ShouldBeOne()
        {
            // Arrange
            var categories = MerchantCatalog.Categories;

            // Act & Assert
            var totalWeight = categories.Sum(c => c.Weight);

            totalWeight.Should().BeApproximately(1.0, 0.0001);
        }
    }
}

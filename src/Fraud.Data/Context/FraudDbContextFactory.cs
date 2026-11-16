using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fraud.Data.Context
{
    // Design-time only (dotnet ef). Never connects; migrations add just needs the provider configured.
    public class FraudDbContextFactory : IDesignTimeDbContextFactory<FraudDbContext>
    {
        public FraudDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<FraudDbContext>()
                .UseNpgsql("Host=localhost;Database=fraud-design-time", con => con.UseVector())
                .Options;

            return new FraudDbContext(options);
        }
    }
}

using Fraud.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fraud.Data.ServiceRegistration
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Context.FraudDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("FraudDB"), con => con.UseVector());
            });

            services.AddScoped<ITransactionRepository, TransactionRepository>()
                    .AddScoped<ICaseRepository, CaseRepository>()
                    .AddScoped<IGroundTruthRepository, GroundTruthRepository>();


            return services;
        }
    }
}

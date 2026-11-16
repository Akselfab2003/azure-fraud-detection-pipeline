using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fraud.Data.ServiceRegistration
{
    public static class ServicesConfiguration
    {
        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Context.FraudDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("FraudDB"), con => con.UseVector());
            });
            return services.BuildServiceProvider();
        }
    }
}

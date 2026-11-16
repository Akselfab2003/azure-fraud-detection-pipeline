using Fraud.Data.Context;
using Fraud.Data.ServiceRegistration;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Scoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            builder.AddAzureServiceBusClient("ServiceBus");
            builder.Services.ConfigureDatabaseServices(builder.Configuration);

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();

            if (builder.Environment.IsDevelopment())
            {
                using var scope = host.Services.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FraudDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                dbContext.Database.EnsureCreated();

                if (dbContext.Database.MigrateAsync().IsCompletedSuccessfully)
                {
                    logger.LogInformation("Database migration completed successfully.");
                }
                else
                {
                    logger.LogError("Database migration failed.");
                }
            }
            host.Run();
        }
    }
}

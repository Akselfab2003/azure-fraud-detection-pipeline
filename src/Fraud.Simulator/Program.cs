using Fraud.Data.Context;
using Fraud.Data.ServiceRegistration;
using Fraud.Simulator.Configuration;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Patterns;
using Fraud.Simulator.Population;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fraud.Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.AddServiceDefaults();
            builder.AddAzureServiceBusClient("ServiceBus");
            builder.Services.ConfigureDatabaseServices(builder.Configuration);


            builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection("SimulatorOptions"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SimulatorOptions>>().Value);

            builder.Services.AddSingleton(TimeProvider.System);

            // The population is built once from the seed and shared by everything below.
            builder.Services.AddSingleton(sp => new PopulationBuilder(sp.GetRequiredService<SimulatorOptions>()).Build());
            builder.Services.AddSingleton<ActivityCurve>();
            builder.Services.AddSingleton<NormalTraffic>();

            builder.Services.AddSingleton<IFraudPattern, CardTestingPattern>()
                            .AddSingleton<IFraudPattern, ImpossibleTravelPattern>()
                            .AddSingleton<IFraudPattern, NewMerchantBurstPattern>()
                            .AddSingleton<IFraudPattern, AccountTakeoverPattern>();

            builder.Services.AddSingleton<TrafficGenerator>();
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

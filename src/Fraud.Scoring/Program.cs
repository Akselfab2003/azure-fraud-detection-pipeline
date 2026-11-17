using Fraud.Data.ServiceRegistration;
using Fraud.Scoring.Handling;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            builder.AddAzureServiceBusClient("ServiceBus");
            builder.ConfigureDatabaseServices();

            builder.AddRedisClient("Redis");

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddScoped<RiskScore>()
                            .AddScoped<TransactionHandler>();

            builder.Services.AddScoped<IRule, VelocityRule>()
                            .AddScoped<IRule, AmountBaselineRule>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.MigrateDatabaseAsync().GetAwaiter().GetResult();

            host.Run();
        }
    }
}

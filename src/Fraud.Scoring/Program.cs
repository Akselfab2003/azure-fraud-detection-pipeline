using Fraud.Data.ServiceRegistration;
using Fraud.Scoring.Handling;
using Fraud.Scoring.Infrastructure;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();
            builder.Services.AddSingleton(TimeProvider.System);

            builder.AddAzureServiceBusClient("ServiceBus");
            builder.ConfigureDatabaseServices();
            builder.Services.AddStackExchangeRedisCache(setup =>
            {
                setup.Configuration = builder.Configuration.GetConnectionString("Redis");
                setup.InstanceName = "FraudScoring:";
            });
            builder.Services.Configure<ScoringOptions>(builder.Configuration.GetSection(ScoringOptions.SectionName));


            builder.Services.AddScoped<RiskScorer>()
                            .AddScoped<CaseWriter>()
                            .AddScoped<TransactionHandler>();

            builder.Services.AddScoped<ICustomerStateStore, RedisCustomerStateStore>();

            builder.Services.AddScoped<IRule, VelocityRule>()
                            .AddScoped<IRule, AmountBaselineRule>()
                            .AddScoped<IRule, ImpossibleTravelRule>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.MigrateDatabaseAsync().GetAwaiter().GetResult();

            host.Run();
        }
    }
}

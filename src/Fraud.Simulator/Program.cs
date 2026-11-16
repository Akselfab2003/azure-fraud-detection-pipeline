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
            builder.ConfigureDatabaseServices();


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
            host.MigrateDatabaseAsync().GetAwaiter().GetResult();

            host.Run();
        }
    }
}

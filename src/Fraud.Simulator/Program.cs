using Fraud.Simulator.Configuration;
using Fraud.Simulator.Patterns;

namespace Fraud.Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.AddServiceDefaults();
            builder.AddAzureServiceBusClient("ServiceBus");

            builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection("SimulatorOptions"));

            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddSingleton<IFraudPattern, CardTestingPattern>()
                            .AddSingleton<IFraudPattern, ImpossibleTravelPattern>()
                            .AddSingleton<IFraudPattern, NewMerchantBurstPattern>()
                            .AddSingleton<IFraudPattern, AccountTakeoverPattern>();


            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}

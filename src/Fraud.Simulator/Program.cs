using Fraud.Simulator.Configuration;

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

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}

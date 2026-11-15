namespace Fraud.Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.AddServiceDefaults();
            builder.AddAzureServiceBusClient("ServiceBus");

            builder.Services.Configure<Settings>(builder.Configuration.GetSection("SimulatorSettings"));

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}

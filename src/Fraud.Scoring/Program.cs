namespace Fraud.Scoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            builder.AddAzureServiceBusClient("ServiceBus");

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}

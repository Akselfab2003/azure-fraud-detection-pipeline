namespace Fraud.Scoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            builder.AddAzureServiceBusClient("ServiceBus", options =>
            {
                options.DisableTracing = false;
            });

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}

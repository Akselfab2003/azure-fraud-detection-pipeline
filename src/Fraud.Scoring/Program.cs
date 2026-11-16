using Fraud.Data.ServiceRegistration;

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
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.MigrateDatabaseAsync().GetAwaiter().GetResult();

            host.Run();
        }
    }
}

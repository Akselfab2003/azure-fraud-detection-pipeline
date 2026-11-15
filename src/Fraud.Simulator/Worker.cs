using Azure.Messaging.ServiceBus;
using Bogus;
using Fraud.Contracts;
using Microsoft.Extensions.Options;

namespace Fraud.Simulator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly Settings _settings;

        public Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient, IOptions<Settings> settings)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using ServiceBusSender sender = _serviceBusClient.CreateSender("transactions-queue");
            var delay = TimeSpan.FromSeconds(1.0 / _settings.Rps);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    Transaction transaction = GenerateTransaction();
                    await sender.SendMessageAsync(new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(transaction)), stoppingToken);
                }

                await Task.Delay(delay, stoppingToken);
            }
        }


        private Transaction GenerateTransaction()
        {
            var TransactionFaker = new Faker<Transaction>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.CustomerId, f => Guid.NewGuid())
                .RuleFor(t => t.MerchantId, f => Guid.NewGuid())
                .RuleFor(t => t.Amount, f => f.Finance.Amount(1, 1000))
                .RuleFor(t => t.Timestamp, f => f.Date.Recent())
                .RuleFor(t => t.IsFraud, false);

            return TransactionFaker.Generate();
        }
    }
}

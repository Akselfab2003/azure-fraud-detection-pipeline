using Azure.Messaging.ServiceBus;
using Fraud.Contracts;

namespace Fraud.Scoring
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using ServiceBusProcessor processor = _serviceBusClient.CreateProcessor("transactions-queue", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing message: {ErrorSource}", args.ErrorSource);
            await Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            Transaction? transaction = System.Text.Json.JsonSerializer.Deserialize<Transaction>(args.Message.Body);
            if (transaction != null)
            {
                _logger.LogInformation("Processing transaction {TransactionId} for customer {CustomerId} with amount {Amount}",
                                   transaction.Id,
                                   transaction.CustomerId,
                                   transaction.Amount);
                await args.CompleteMessageAsync(args.Message);
            }
        }
    }
}

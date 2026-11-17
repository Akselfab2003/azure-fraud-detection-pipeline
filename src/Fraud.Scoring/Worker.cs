using Azure.Messaging.ServiceBus;
using Fraud.Contracts;
using Fraud.Scoring.Handling;

namespace Fraud.Scoring
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private TransactionHandler _Handler = null!;
        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            ServiceBusClient _serviceBusClient = scope.ServiceProvider.GetRequiredService<ServiceBusClient>();
            _Handler = scope.ServiceProvider.GetRequiredService<TransactionHandler>();
            await using ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(QueueNames.Transactions, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
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
            TransactionCreated? transaction = System.Text.Json.JsonSerializer.Deserialize<TransactionCreated>(args.Message.Body);

            if (transaction != null)
            {
                await _Handler.HandleTransaction(transaction, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        }
    }
}

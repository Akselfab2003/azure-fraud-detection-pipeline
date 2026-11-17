using Azure.Messaging.ServiceBus;
using Fraud.Contracts;
using Fraud.Scoring.Handling;

namespace Fraud.Scoring
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                using var scope = _scopeFactory.CreateAsyncScope();
                TransactionHandler _Handler = scope.ServiceProvider.GetRequiredService<TransactionHandler>();
                await _Handler.HandleTransaction(transaction, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                return;
            }
            else
            {
                await args.DeadLetterMessageAsync(args.Message, "Invalid message format", cancellationToken: args.CancellationToken);
            }
        }
    }
}

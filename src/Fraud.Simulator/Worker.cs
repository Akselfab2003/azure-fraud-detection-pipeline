using Azure.Messaging.ServiceBus;
using Fraud.Contracts;
using Fraud.Data.Repositories;
using Fraud.Simulator.Configuration;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Reporting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Fraud.Simulator
{
    public class Worker(ILogger<Worker> logger,
                        ServiceBusClient serviceBusClient,
                        TrafficGenerator generator,
                        IServiceScopeFactory scopeFactory,
                        IOptionsMonitor<SimulatorOptions> options,
                        TimeProvider time
                       ) : BackgroundService
    {
        private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(10);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using ServiceBusSender sender = serviceBusClient.CreateSender(QueueNames.Transactions);
            await using var scope = scopeFactory.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IGroundTruthRepository groundTruthRepository = scope.ServiceProvider.GetRequiredService<IGroundTruthRepository>();
            var stats = new SendStats();
            var lastLog = time.GetTimestamp();

            foreach (var tx in generator.Stream())
            {
                // Only the transaction goes on the wire; the fraud label stays in the simulator.
                var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(tx.Message))
                {
                    MessageId = tx.Message.TransactionId.ToString(),
                    ContentType = "application/json",
                };
                await groundTruthRepository.AddGroundTruthAsync(tx.Message.TransactionId, tx.IsFraud, tx.Pattern.ToString());
                await unitOfWork.SaveChangesAsync(stoppingToken);
                await sender.SendMessageAsync(message, stoppingToken);
                stats.Record(tx);

                var elapsed = time.GetElapsedTime(lastLog);
                if (elapsed >= LogInterval)
                {
                    LogSummary(stats.Drain(), elapsed);
                    lastLog = time.GetTimestamp();
                }

                // Rate is re-read every iteration, so a config change takes effect without a restart.
                var rate = Math.Max(options.CurrentValue.RatePerSecond, 0.01);
                await Task.Delay(TimeSpan.FromSeconds(1.0 / rate), time, stoppingToken);
            }
        }

        private void LogSummary(SendSummary summary, TimeSpan elapsed)
        {
            var byPattern = summary.ByPattern.Count == 0
                ? "none"
                : string.Join(", ", summary.ByPattern.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));

            logger.LogInformation(
                "Sent {Sent} transactions in {Seconds:F0}s ({Rate:F1}/s); fraud {Fraud} ({Share:P1}): {ByPattern}",
                summary.Sent,
                elapsed.TotalSeconds,
                summary.Sent / elapsed.TotalSeconds,
                summary.Fraud,
                summary.Sent == 0 ? 0 : (double)summary.Fraud / summary.Sent,
                byPattern);
        }
    }
}

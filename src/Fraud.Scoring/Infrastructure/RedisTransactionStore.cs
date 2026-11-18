using Fraud.Contracts;
using StackExchange.Redis;
using System.Text.Json;

namespace Fraud.Scoring.Infrastructure
{
    public class RedisTransactionStore : ITransactionStore
    {
        private readonly TimeSpan TransactionLifetime = TimeSpan.FromHours(24);
        private readonly ILogger<RedisCustomerStateStore> _logger;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly TimeProvider _timeProvider;

        public RedisTransactionStore(ILogger<RedisCustomerStateStore> logger, IConnectionMultiplexer connectionMultiplexer, TimeProvider timeProvider)
        {
            _logger = logger;
            _connectionMultiplexer = connectionMultiplexer;
            _timeProvider = timeProvider;
        }


        private string CustomerTransactionKey(Guid customer) => $"transaction:{customer}";

        public async Task WriteTransactionToCache(TransactionCreated tx, CancellationToken ct = default)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(tx);
            var key = CustomerTransactionKey(tx.CustomerId);
            var db = _connectionMultiplexer.GetDatabase();
            await db.SortedSetAddAsync(key: key,
                                       member: json,
                                       score: _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

            await db.KeyExpireAsync(key: key,
                                    expiry: TransactionLifetime);
        }


        public async Task<List<TransactionCreated>> ReadTransactionsFromCache(Guid CustomerId, CancellationToken ct = default)
        {
            var key = CustomerTransactionKey(CustomerId);
            var db = _connectionMultiplexer.GetDatabase();
            var cache_values = await db.SortedSetRangeByScoreAsync(key: key,
                                                                   start: 0,
                                                                   stop: _timeProvider.GetUtcNow()
                                                                                      .ToUnixTimeMilliseconds());

            var transactions = new List<TransactionCreated>(cache_values.Length);

            foreach (var v in cache_values)
                transactions.Add(JsonSerializer.Deserialize(((ReadOnlyMemory<byte>)v).Span, FraudJson.Default.TransactionCreated)!);
            return transactions;
        }
    }
}

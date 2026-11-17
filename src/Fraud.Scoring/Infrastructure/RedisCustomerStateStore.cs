using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Caching.Distributed;

namespace Fraud.Scoring.Infrastructure
{
    public class RedisCustomerStateStore : ICustomerStateStore
    {
        private readonly ILogger<RedisCustomerStateStore> _logger;
        private readonly IDistributedCache _cache;
        private DistributedCacheEntryOptions cacheOptions => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public RedisCustomerStateStore(ILogger<RedisCustomerStateStore> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }


        public async Task UpsertCustomerState(Guid CustomerId, CustomerState customerState, CancellationToken ct)
        {
            var key = $"customer:{CustomerId}";
            var json = System.Text.Json.JsonSerializer.Serialize(customerState);
            await _cache.SetStringAsync(key: key, value: json, options: cacheOptions, token: ct);
        }

        public async Task<CustomerState?> GetCustomerState(Guid CustomerId, CancellationToken ct)
        {
            var key = $"customer:{CustomerId}";
            var json = await _cache.GetStringAsync(key: key, token: ct);

            if (json is null)
            {
                return null;
            }

            return System.Text.Json.JsonSerializer.Deserialize<CustomerState>(json);
        }
    }
}

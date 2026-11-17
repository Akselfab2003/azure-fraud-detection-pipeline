using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Caching.Distributed;

namespace Fraud.Scoring.Infrastructure
{
    public class RedisCustomerStateStore : ICustomerStateStore
    {
        private readonly ILogger<RedisCustomerStateStore> _logger;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1),
            SlidingExpiration = TimeSpan.FromHours(1)
        };

        public RedisCustomerStateStore(ILogger<RedisCustomerStateStore> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }


        public async Task UpsertCustomerState(Guid CustomerId, CustomerState customerState, CancellationToken ct)
        {
            try
            {
                var key = $"customer:{CustomerId}";
                var json = System.Text.Json.JsonSerializer.Serialize(customerState);
                await _cache.SetStringAsync(key: key, value: json, options: _cacheOptions, token: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting customer state");
            }
        }

        public async Task<CustomerState?> GetCustomerState(Guid CustomerId, CancellationToken ct)
        {
            try
            {
                var key = $"customer:{CustomerId}";
                var json = await _cache.GetStringAsync(key: key, token: ct);

                if (json is null)
                {
                    return null;
                }

                return System.Text.Json.JsonSerializer.Deserialize<CustomerState>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer state");
                return null;
            }
        }
    }
}

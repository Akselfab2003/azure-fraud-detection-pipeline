using Fraud.Scoring.Infrastructure;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Fraud.Scoring.Tests.Integration;

[Collection(InfraCollection.Name)]
public class RedisCustomerStateStoreTests(InfraFixture infra)
{
    private RedisCustomerStateStore Store() =>
        new(NullLogger<RedisCustomerStateStore>.Instance, infra.CreateCache());

    [Fact]
    public async Task Unknown_customer_returns_null()
    {
        var state = await Store().GetCustomerState(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        Assert.Null(state);
    }

    [Fact]
    public async Task State_round_trips_through_redis()
    {
        var customerId = Guid.CreateVersion7();
        var saved = TestData.State(TestData.RecentTimes(3), average: 123.45m, count: 7);

        await Store().UpsertCustomerState(customerId, saved, TestContext.Current.CancellationToken);
        var loaded = await Store().GetCustomerState(customerId, TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(saved.RecentTransactionTimes, loaded.RecentTransactionTimes);
        Assert.Equal(saved.LastCountry, loaded.LastCountry);
        Assert.Equal(saved.LastTransactionAt, loaded.LastTransactionAt);
        Assert.Equal(saved.AverageAmount, loaded.AverageAmount);
        Assert.Equal(saved.TransactionCount, loaded.TransactionCount);
    }

    [Fact]
    public async Task Upsert_overwrites_the_previous_state()
    {
        var customerId = Guid.CreateVersion7();
        var store = Store();

        await store.UpsertCustomerState(customerId, TestData.State(count: 1, average: 10m), TestContext.Current.CancellationToken);
        await store.UpsertCustomerState(customerId, TestData.State(count: 2, average: 20m), TestContext.Current.CancellationToken);

        var loaded = await store.GetCustomerState(customerId, TestContext.Current.CancellationToken);
        Assert.Equal(2, loaded?.TransactionCount);
    }

    [Fact]
    public async Task Customers_do_not_share_state()
    {
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();

        await Store().UpsertCustomerState(a, TestData.State(count: 5), TestContext.Current.CancellationToken);

        Assert.Null(await Store().GetCustomerState(b, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task State_expires_so_it_cannot_grow_forever()
    {
        var customerId = Guid.CreateVersion7();
        await Store().UpsertCustomerState(customerId, TestData.State(count: 1), TestContext.Current.CancellationToken);

        await using var redis = await ConnectionMultiplexer.ConnectAsync(infra.RedisConnectionString);
        var ttl = await redis.GetDatabase().KeyTimeToLiveAsync($"{InfraFixture.RedisInstanceName}customer:{customerId}");

        Assert.NotNull(ttl);
        Assert.InRange(ttl.Value, TimeSpan.Zero, TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Redis_failure_is_not_swallowed()
    {
        // Nothing listens on port 1. abortConnect=false keeps the failure on the call rather than the constructor.
        var cache = new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = "localhost:1,abortConnect=false,connectTimeout=500,syncTimeout=500,asyncTimeout=500",
        }));
        var store = new RedisCustomerStateStore(NullLogger<RedisCustomerStateStore>.Instance, cache);

        await Assert.ThrowsAnyAsync<Exception>(() => store.GetCustomerState(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }
}

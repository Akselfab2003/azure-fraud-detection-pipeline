using Fraud.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Fraud.Scoring.Tests.Integration;

// One pgvector Postgres and one Redis container per test run. Tests use unique customer ids instead of cleaning up.
public sealed class InfraFixture : IAsyncLifetime
{
    public const string RedisInstanceName = "FraudScoringTests:";

    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("pgvector/pgvector:0.8.7-pg18-trixie").Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    private DbContextOptions<FraudDbContext> _dbOptions = null!;

    public string RedisConnectionString => _redis.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        _dbOptions = new DbContextOptionsBuilder<FraudDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(), npgsql => npgsql.UseVector())
            .Options;

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }

    public FraudDbContext CreateDbContext() => new(_dbOptions);

    public IDistributedCache CreateCache() => new RedisCache(Options.Create(new RedisCacheOptions
    {
        Configuration = RedisConnectionString,
        InstanceName = RedisInstanceName,
    }));
}

[CollectionDefinition(Name)]
public sealed class InfraCollection : ICollectionFixture<InfraFixture>
{
    public const string Name = "infra";
}

using Fraud.Contracts.Enums;
using Fraud.Data.Context;
using Fraud.Data.Entities;
using Fraud.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Fraud.Data.Tests;

// One pgvector Postgres container per test run, migrated once. Tests use unique ids instead of cleaning up.
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("pgvector/pgvector:0.8.7-pg18-trixie").Build();

    private DbContextOptions<FraudDbContext> _options = null!;

    public string ConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        _options = new DbContextOptionsBuilder<FraudDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.UseVector())
            .Options;

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync() => _postgres.DisposeAsync();

    public FraudDbContext CreateContext() => new(_options);
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}

public static class TestData
{
    public static TransactionEntity Transaction(Guid? id = null, Guid? customerId = null, DateTimeOffset? at = null)
    {
        var now = at ?? DateTimeOffset.UtcNow;
        return new TransactionEntity
        {
            Id = id ?? Guid.CreateVersion7(),
            CustomerId = customerId ?? Guid.CreateVersion7(),
            MerchantId = Guid.CreateVersion7(),
            MerchantCategory = "electronics",
            Amount = 1234.50m,
            Currency = "DKK",
            Country = "DK",
            Channel = Channel.Online,
            OccurredAt = now,
            RiskScore = 80m,
            Decision = Decision.Review,
            ScoredAt = now
        };
    }

    public static CaseEntity Case(Guid transactionId, Guid? id = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new CaseEntity
        {
            Id = id ?? Guid.CreateVersion7(),
            TransactionId = transactionId,
            RiskScore = 80m,
            Decision = Decision.Review,
            Reasons = [new CaseReason("VELOCITY_HIGH", 40, "6 transactions in 10 minutes")],
            RuleSetVersion = "rules-v1",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static CaseEventEntity Event(Guid caseId, CaseEventType type = CaseEventType.Opened, DateTimeOffset? at = null) => new()
    {
        Id = Guid.CreateVersion7(),
        CaseId = caseId,
        Type = type,
        Actor = "scoring",
        At = at ?? DateTimeOffset.UtcNow
    };
}

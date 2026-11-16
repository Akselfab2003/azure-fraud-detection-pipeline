using Fraud.Contracts.Enums;
using Fraud.Data.Context;
using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Fraud.Data.Tests;

// Integration test: needs Docker. Runs against a throwaway pgvector Postgres started by Testcontainers.
public class CaseStorageTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("pgvector/pgvector:0.8.7-pg18-trixie").Build();

    public ValueTask InitializeAsync() => new(_postgres.StartAsync());

    public ValueTask DisposeAsync() => _postgres.DisposeAsync();

    [Fact]
    public async Task Case_written_can_be_read_back_with_its_transaction_and_events()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionString = _postgres.GetConnectionString();
        var options = new DbContextOptionsBuilder<FraudDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
            .Options;

        var transactionId = Guid.CreateVersion7();
        var caseId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;

        await using (var write = new FraudDbContext(options))
        {
            await write.Database.MigrateAsync(ct);

            write.Transactions.Add(new TransactionEntity
            {
                Id = transactionId,
                CustomerId = Guid.CreateVersion7(),
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
            });
            write.Cases.Add(new CaseEntity
            {
                Id = caseId,
                TransactionId = transactionId,
                RiskScore = 80m,
                Decision = Decision.Review,
                Reasons = [new CaseReason("VELOCITY_HIGH", 40, "6 transactions in 10 minutes")],
                RuleSetVersion = "rules-v1",
                CreatedAt = now,
                UpdatedAt = now,
                Events = [new CaseEventEntity { Id = Guid.CreateVersion7(), Type = Data.Enums.CaseEventType.Opened, Actor = "scoring", At = now }]
            });
            write.GroundTruth.Add(new GroundTruthEntity { TransactionId = transactionId, IsFraud = true, Pattern = "CardTesting" });
            await write.SaveChangesAsync(ct);
        }

        await using var read = new FraudDbContext(options);
        var loaded = await read.Cases
            .Include(c => c.Transaction)
            .Include(c => c.Events)
            .SingleAsync(c => c.Id == caseId, ct);

        Assert.Equal(transactionId, loaded.Transaction.Id);
        Assert.Equal(1234.50m, loaded.Transaction.Amount);
        Assert.Equal(Data.Enums.CaseStatus.Open, loaded.Status);
        Assert.Equal("VELOCITY_HIGH", Assert.Single(loaded.Reasons).Code);
        Assert.Single(loaded.Events);
        Assert.NotEqual(0u, loaded.Version);

        // The label lives in its own table, not on the case or transaction
        var truth = await read.GroundTruth.SingleAsync(g => g.TransactionId == transactionId, ct);
        Assert.True(truth.IsFraud);
    }
}

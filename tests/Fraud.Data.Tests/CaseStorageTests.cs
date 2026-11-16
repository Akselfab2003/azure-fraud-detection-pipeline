using Fraud.Data.Entities;
using Fraud.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class CaseStorageTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Case_written_can_be_read_back_with_its_transaction_and_events()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        @case.Events = [TestData.Event(@case.Id)];

        await using (var write = postgres.CreateContext())
        {
            write.Transactions.Add(transaction);
            write.Cases.Add(@case);
            write.GroundTruth.Add(new GroundTruthEntity { TransactionId = transaction.Id, IsFraud = true, Pattern = "CardTesting" });
            await write.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        var loaded = await read.Cases
            .Include(c => c.Transaction)
            .Include(c => c.Events)
            .SingleAsync(c => c.Id == @case.Id, ct);

        Assert.Equal(transaction.Id, loaded.Transaction.Id);
        Assert.Equal(1234.50m, loaded.Transaction.Amount);
        Assert.Equal(CaseStatus.Open, loaded.Status);
        Assert.Equal("VELOCITY_HIGH", Assert.Single(loaded.Reasons).Code);
        Assert.Single(loaded.Events);
        Assert.NotEqual(0u, loaded.Version);

        // The label lives in its own table, not on the case or transaction
        var truth = await read.GroundTruth.SingleAsync(g => g.TransactionId == transaction.Id, ct);
        Assert.True(truth.IsFraud);
    }

    [Fact]
    public async Task Case_reasons_keep_all_fields_and_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        @case.Reasons =
        [
            new CaseReason("VELOCITY_HIGH", 40, "6 in 10 min"),
            new CaseReason("AMOUNT_SPIKE", 35, "8x average")
        ];

        await using (var write = postgres.CreateContext())
        {
            write.Transactions.Add(transaction);
            write.Cases.Add(@case);
            await write.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        var loaded = await read.Cases.SingleAsync(c => c.Id == @case.Id, ct);

        Assert.Equal(@case.Reasons, loaded.Reasons);
    }

    [Fact]
    public async Task Optional_case_fields_round_trip_when_set()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        var labelledAt = DateTimeOffset.UtcNow;
        @case.Status = CaseStatus.ConfirmedFraud;
        @case.Label = CaseLabel.Fraud;
        @case.LabelledBy = "analyst-1";
        @case.LabelledAt = labelledAt;
        @case.LabelNote = "chargeback confirmed";
        @case.AnomalyScore = 0.93;
        @case.ModelVersion = "iforest-2026-11-20";

        await using (var write = postgres.CreateContext())
        {
            write.Transactions.Add(transaction);
            write.Cases.Add(@case);
            await write.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        var loaded = await read.Cases.SingleAsync(c => c.Id == @case.Id, ct);

        Assert.Equal(CaseStatus.ConfirmedFraud, loaded.Status);
        Assert.Equal(CaseLabel.Fraud, loaded.Label);
        Assert.Equal("analyst-1", loaded.LabelledBy);
        Assert.Equal(labelledAt, loaded.LabelledAt!.Value, TimeSpan.FromMilliseconds(1));
        Assert.Equal("chargeback confirmed", loaded.LabelNote);
        Assert.Equal(0.93, loaded.AnomalyScore);
        Assert.Equal("iforest-2026-11-20", loaded.ModelVersion);
    }
}

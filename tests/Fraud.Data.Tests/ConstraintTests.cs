using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class ConstraintTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Duplicate_transaction_id_is_rejected_for_idempotency()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();

        await using (var first = postgres.CreateContext())
        {
            first.Transactions.Add(transaction);
            await first.SaveChangesAsync(ct);
        }

        await using var second = postgres.CreateContext();
        second.Transactions.Add(TestData.Transaction(id: transaction.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Only_one_case_is_allowed_per_transaction()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();

        await using (var first = postgres.CreateContext())
        {
            first.Transactions.Add(transaction);
            first.Cases.Add(TestData.Case(transaction.Id));
            await first.SaveChangesAsync(ct);
        }

        await using var second = postgres.CreateContext();
        second.Cases.Add(TestData.Case(transaction.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Case_for_an_unknown_transaction_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();
        db.Cases.Add(TestData.Case(Guid.CreateVersion7()));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Transaction_with_a_case_cannot_be_deleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();

        await using (var setup = postgres.CreateContext())
        {
            setup.Transactions.Add(transaction);
            setup.Cases.Add(TestData.Case(transaction.Id));
            await setup.SaveChangesAsync(ct);
        }

        await using var db = postgres.CreateContext();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            db.Transactions.Where(t => t.Id == transaction.Id).ExecuteDeleteAsync(ct));
    }

    [Fact]
    public async Task Ground_truth_can_be_written_before_the_transaction_is_scored()
    {
        var ct = TestContext.Current.CancellationToken;
        var transactionId = Guid.CreateVersion7();

        await using var db = postgres.CreateContext();
        db.GroundTruth.Add(new GroundTruthEntity { TransactionId = transactionId, IsFraud = false, Pattern = "None" });
        await db.SaveChangesAsync(ct);

        Assert.True(await db.GroundTruth.AnyAsync(g => g.TransactionId == transactionId, ct));
        Assert.False(await db.Transactions.AnyAsync(t => t.Id == transactionId, ct));
    }

    [Fact]
    public async Task Duplicate_ground_truth_for_a_transaction_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var transactionId = Guid.CreateVersion7();

        await using (var first = postgres.CreateContext())
        {
            first.GroundTruth.Add(new GroundTruthEntity { TransactionId = transactionId, IsFraud = true, Pattern = "CardTesting" });
            await first.SaveChangesAsync(ct);
        }

        await using var second = postgres.CreateContext();
        second.GroundTruth.Add(new GroundTruthEntity { TransactionId = transactionId, IsFraud = true, Pattern = "CardTesting" });

        await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync(ct));
    }
}

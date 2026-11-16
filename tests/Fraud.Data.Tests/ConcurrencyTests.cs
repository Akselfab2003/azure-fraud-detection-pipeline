using Fraud.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class ConcurrencyTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Second_writer_of_a_stale_case_gets_a_concurrency_error()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);

        await using (var setup = postgres.CreateContext())
        {
            setup.Transactions.Add(transaction);
            setup.Cases.Add(@case);
            await setup.SaveChangesAsync(ct);
        }

        await using var analystA = postgres.CreateContext();
        await using var analystB = postgres.CreateContext();
        var a = await analystA.Cases.SingleAsync(c => c.Id == @case.Id, ct);
        var b = await analystB.Cases.SingleAsync(c => c.Id == @case.Id, ct);

        a.Status = CaseStatus.InReview;
        await analystA.SaveChangesAsync(ct);

        b.Status = CaseStatus.Dismissed;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => analystB.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Version_changes_when_a_case_is_updated()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);

        await using var db = postgres.CreateContext();
        db.Transactions.Add(transaction);
        db.Cases.Add(@case);
        await db.SaveChangesAsync(ct);
        var before = @case.Version;

        @case.Status = CaseStatus.InReview;
        await db.SaveChangesAsync(ct);

        Assert.NotEqual(before, @case.Version);
    }
}

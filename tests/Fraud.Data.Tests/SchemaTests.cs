using Fraud.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class SchemaTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Tables_use_snake_case_names()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();

        var tables = await db.Database
            .SqlQueryRaw<string>("SELECT table_name AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync(ct);

        Assert.Contains("transactions", tables);
        Assert.Contains("cases", tables);
        Assert.Contains("case_events", tables);
        Assert.Contains("ground_truth", tables);
    }

    [Fact]
    public async Task Enums_are_stored_as_readable_strings()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        @case.Status = CaseStatus.InReview;

        await using var db = postgres.CreateContext();
        db.Transactions.Add(transaction);
        db.Cases.Add(@case);
        await db.SaveChangesAsync(ct);

        var caseId = @case.Id;
        var transactionId = transaction.Id;
        var status = await db.Database
            .SqlQuery<string>($"SELECT status AS \"Value\" FROM cases WHERE id = {caseId}").SingleAsync(ct);
        var decision = await db.Database
            .SqlQuery<string>($"SELECT decision AS \"Value\" FROM cases WHERE id = {caseId}").SingleAsync(ct);
        var channel = await db.Database
            .SqlQuery<string>($"SELECT channel AS \"Value\" FROM transactions WHERE id = {transactionId}").SingleAsync(ct);

        Assert.Equal("InReview", status);
        Assert.Equal("Review", decision);
        Assert.Equal("Online", channel);
    }

    [Fact]
    public async Task Column_types_match_the_data_model()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();

        var columns = await db.Database.SqlQueryRaw<ColumnType>(
            "SELECT table_name || '.' || column_name AS \"Name\", data_type AS \"Type\" " +
            "FROM information_schema.columns WHERE table_schema = 'public'").ToListAsync(ct);
        var types = columns.ToDictionary(c => c.Name, c => c.Type);

        Assert.Equal("numeric", types["transactions.amount"]);
        Assert.Equal("character", types["transactions.currency"]);
        Assert.Equal("character", types["transactions.country"]);
        Assert.Equal("timestamp with time zone", types["transactions.occurred_at"]);
        Assert.Equal("jsonb", types["cases.reasons"]);
        Assert.Equal("jsonb", types["case_events.data_json"]);
        Assert.Equal("USER-DEFINED", types["cases.embedding"]); // pgvector
    }

    [Fact]
    public async Task Indexes_exist_for_customer_time_and_case_status()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();

        var definitions = await db.Database
            .SqlQueryRaw<string>("SELECT indexdef AS \"Value\" FROM pg_indexes WHERE schemaname = 'public'")
            .ToListAsync(ct);

        Assert.Contains(definitions, d => d.Contains("ON public.transactions") && d.Contains("(customer_id, occurred_at)"));
        Assert.Contains(definitions, d => d.Contains("ON public.cases") && d.Contains("(status)"));
        Assert.Contains(definitions, d => d.Contains("UNIQUE") && d.Contains("ON public.cases") && d.Contains("(transaction_id)"));
    }

    [Fact]
    public async Task All_migrations_are_applied()
    {
        await using var db = postgres.CreateContext();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
    }

    public sealed record ColumnType(string Name, string Type);
}

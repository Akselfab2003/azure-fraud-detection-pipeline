using Fraud.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class CaseEventTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Deleting_a_case_deletes_its_events_but_keeps_the_transaction()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        @case.Events = [TestData.Event(@case.Id), TestData.Event(@case.Id, CaseEventType.Explained)];

        await using (var setup = postgres.CreateContext())
        {
            setup.Transactions.Add(transaction);
            setup.Cases.Add(@case);
            await setup.SaveChangesAsync(ct);
        }

        await using (var db = postgres.CreateContext())
        {
            var loaded = await db.Cases.SingleAsync(c => c.Id == @case.Id, ct);
            db.Cases.Remove(loaded);
            await db.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        Assert.False(await read.CaseEvents.AnyAsync(e => e.CaseId == @case.Id, ct));
        Assert.True(await read.Transactions.AnyAsync(t => t.Id == transaction.Id, ct));
    }

    [Fact]
    public async Task Event_data_json_round_trips_as_jsonb()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        var @event = TestData.Event(@case.Id, CaseEventType.StatusChanged);
        @event.DataJson = "{\"old\":\"Open\",\"new\":\"InReview\"}";
        @case.Events = [@event];

        await using (var setup = postgres.CreateContext())
        {
            setup.Transactions.Add(transaction);
            setup.Cases.Add(@case);
            await setup.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        var loaded = await read.CaseEvents.SingleAsync(e => e.Id == @event.Id, ct);

        Assert.Equal(CaseEventType.StatusChanged, loaded.Type);
        Assert.Contains("\"new\": \"InReview\"", loaded.DataJson); // jsonb normalises spacing
    }

    [Fact]
    public async Task Case_events_can_be_read_in_time_order()
    {
        var ct = TestContext.Current.CancellationToken;
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        var t0 = DateTimeOffset.UtcNow;
        @case.Events =
        [
            TestData.Event(@case.Id, CaseEventType.Labelled, t0.AddMinutes(2)),
            TestData.Event(@case.Id, CaseEventType.Opened, t0),
            TestData.Event(@case.Id, CaseEventType.Explained, t0.AddMinutes(1))
        ];

        await using (var setup = postgres.CreateContext())
        {
            setup.Transactions.Add(transaction);
            setup.Cases.Add(@case);
            await setup.SaveChangesAsync(ct);
        }

        await using var read = postgres.CreateContext();
        var types = await read.CaseEvents
            .Where(e => e.CaseId == @case.Id)
            .OrderBy(e => e.At)
            .Select(e => e.Type)
            .ToListAsync(ct);

        Assert.Equal([CaseEventType.Opened, CaseEventType.Explained, CaseEventType.Labelled], types);
    }
}

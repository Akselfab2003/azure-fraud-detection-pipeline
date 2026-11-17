using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Data.Entities;
using Fraud.Data.Repositories;
using Fraud.Scoring.Handling;
using Fraud.Scoring.Infrastructure;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fraud.Scoring.Tests.Integration;

// Real Postgres + real Redis, wired the same way as Program.cs.
[Collection(InfraCollection.Name)]
public class TransactionHandlerTests(InfraFixture infra)
{
    private sealed class ThrowingStateStore : ICustomerStateStore
    {
        public Task<CustomerState?> GetCustomerState(Guid customerId, CancellationToken ct) =>
            throw new InvalidOperationException("redis is down");

        public Task UpsertCustomerState(Guid customerId, CustomerState customerState, CancellationToken ct) =>
            throw new InvalidOperationException("redis is down");
    }

    // Simulates two deliveries racing: the idempotency check sees nothing, the insert then hits the unique key.
    private sealed class BlindToExistingTransactions(ITransactionRepository inner) : ITransactionRepository
    {
        public Task<TransactionEntity> AddTransactionAsync(TransactionEntity transaction) => inner.AddTransactionAsync(transaction);
        public Task<IEnumerable<TransactionEntity>> GetAllTransactionsAsync() => inner.GetAllTransactionsAsync();
        public Task<TransactionEntity?> GetTransactionByIdAsync(Guid transactionId) => Task.FromResult<TransactionEntity?>(null);
    }

    private async Task Handle(
        TransactionCreated tx,
        ScoringOptions? scoring = null,
        ICustomerStateStore? stateStore = null,
        Func<ITransactionRepository, ITransactionRepository>? wrapTransactions = null)
    {
        var options = TestData.Options(scoring);
        await using var db = infra.CreateDbContext();
        ITransactionRepository transactions = new TransactionRepository(NullLogger<TransactionRepository>.Instance, db);
        transactions = wrapTransactions?.Invoke(transactions) ?? transactions;
        var time = new FakeTimeProvider(TestData.Now);

        var handler = new TransactionHandler(
            new RiskScorer([new VelocityRule(options), new AmountBaselineRule(options), new ImpossibleTravelRule(options)], options),
            transactions,
            stateStore ?? new RedisCustomerStateStore(NullLogger<RedisCustomerStateStore>.Instance, infra.CreateCache()),
            new CaseWriter(new CaseRepository(NullLogger<CaseRepository>.Instance, db), time, options),
            time,
            new UnitOfWork(NullLogger<UnitOfWork>.Instance, db),
            options);

        await handler.HandleTransaction(tx, TestContext.Current.CancellationToken);
    }

    private async Task<TransactionEntity?> FindTransaction(Guid id)
    {
        await using var db = infra.CreateDbContext();
        return await db.Transactions.SingleOrDefaultAsync(t => t.Id == id);
    }

    private async Task<CaseEntity?> FindCase(Guid transactionId)
    {
        await using var db = infra.CreateDbContext();
        return await db.Cases.SingleOrDefaultAsync(c => c.TransactionId == transactionId);
    }

    private async Task<CustomerState?> FindState(Guid customerId) =>
        await new RedisCustomerStateStore(NullLogger<RedisCustomerStateStore>.Instance, infra.CreateCache())
            .GetCustomerState(customerId, TestContext.Current.CancellationToken);

    [Fact]
    public async Task First_transaction_is_stored_allowed_and_starts_the_customer_state()
    {
        var tx = TestData.Tx(amount: 250m);

        await Handle(tx);

        var stored = await FindTransaction(tx.TransactionId);
        Assert.NotNull(stored);
        Assert.Equal(Decision.Allow, stored.Decision);
        Assert.Equal(0m, stored.RiskScore);
        Assert.Null(await FindCase(tx.TransactionId));

        var state = await FindState(tx.CustomerId);
        Assert.NotNull(state);
        Assert.Equal(1, state.TransactionCount);
        Assert.Equal(250m, state.AverageAmount);
        Assert.Equal([tx.OccurredAt], state.RecentTransactionTimes);
    }

    [Fact]
    public async Task Redelivered_message_is_processed_once()
    {
        var tx = TestData.Tx();

        await Handle(tx);
        await Handle(tx);

        await using var db = infra.CreateDbContext();
        Assert.Equal(1, await db.Transactions.CountAsync(t => t.Id == tx.TransactionId, TestContext.Current.CancellationToken));
        Assert.Equal(1, (await FindState(tx.CustomerId))?.TransactionCount);
    }

    [Fact]
    public async Task Duplicate_insert_race_is_treated_as_already_processed()
    {
        var tx = TestData.Tx();
        await Handle(tx);

        // Second delivery slips past the idempotency check; the unique key on the transaction id must catch it.
        await Handle(tx, wrapTransactions: t => new BlindToExistingTransactions(t));

        await using var db = infra.CreateDbContext();
        Assert.Equal(1, await db.Transactions.CountAsync(t => t.Id == tx.TransactionId, TestContext.Current.CancellationToken));
        Assert.Equal(1, (await FindState(tx.CustomerId))?.TransactionCount);
    }

    [Fact]
    public async Task Redis_failure_propagates_and_nothing_is_stored()
    {
        var tx = TestData.Tx();

        // The exception must reach the caller so the message is retried, not completed.
        await Assert.ThrowsAsync<InvalidOperationException>(() => Handle(tx, stateStore: new ThrowingStateStore()));

        Assert.Null(await FindTransaction(tx.TransactionId));
    }

    [Fact]
    public async Task Impossible_travel_opens_a_case_when_it_is_enough_on_its_own()
    {
        var customer = Guid.CreateVersion7();
        var scoring = new ScoringOptions { ReviewThreshold = 40 };

        await Handle(TestData.Tx(100m, TestData.Now, customer, country: "DK"), scoring);
        var abroad = TestData.Tx(100m, TestData.Now.AddMinutes(15), customer, country: "BR");
        await Handle(abroad, scoring);

        var @case = await FindCase(abroad.TransactionId);
        Assert.NotNull(@case);
        Assert.Contains(@case.Reasons, r => r.Code == "IMPOSSIBLE_TRAVEL");
    }

    [Fact]
    public async Task State_accumulates_across_transactions()
    {
        var customer = Guid.CreateVersion7();

        await Handle(TestData.Tx(100m, TestData.Now, customer));
        await Handle(TestData.Tx(300m, TestData.Now.AddMinutes(1), customer));

        var state = await FindState(customer);
        Assert.Equal(2, state?.TransactionCount);
        Assert.Equal(200m, state?.AverageAmount);
        Assert.Equal(2, state?.RecentTransactionTimes.Count);
    }

    [Fact]
    public async Task Velocity_burst_opens_a_case_with_the_velocity_reason()
    {
        var customer = Guid.CreateVersion7();
        // Velocity alone is 40 points, so lower the case threshold to let it open a case.
        var scoring = new ScoringOptions { ReviewThreshold = 40 };

        // Five transactions in five minutes are within the limit...
        for (var i = 0; i < 5; i++)
        {
            var ok = TestData.Tx(100m, TestData.Now.AddMinutes(i), customer);
            await Handle(ok, scoring);
            Assert.Null(await FindCase(ok.TransactionId));
        }

        // ...the sixth is over it.
        var sixth = TestData.Tx(100m, TestData.Now.AddMinutes(5), customer);
        await Handle(sixth, scoring);

        var @case = await FindCase(sixth.TransactionId);
        Assert.NotNull(@case);
        Assert.Equal(Decision.Review, @case.Decision);
        Assert.Equal(40, @case.RiskScore);
        Assert.Contains(@case.Reasons, r => r.Code == "VELOCITY_HIGH");
    }

    [Fact]
    public async Task Amount_spike_and_velocity_together_open_a_case_with_both_reasons()
    {
        var customer = Guid.CreateVersion7();
        for (var i = 0; i < 5; i++)
            await Handle(TestData.Tx(100m, TestData.Now.AddMinutes(i), customer));

        var spike = TestData.Tx(1000m, TestData.Now.AddMinutes(5), customer);
        await Handle(spike);

        var stored = await FindTransaction(spike.TransactionId);
        Assert.Equal(Decision.Review, stored?.Decision);
        Assert.Equal(70m, stored?.RiskScore);

        var @case = await FindCase(spike.TransactionId);
        Assert.NotNull(@case);
        Assert.Equal(["AMOUNT_SPIKE", "VELOCITY_HIGH"], @case.Reasons.Select(r => r.Code).Order());
    }

    [Fact]
    public async Task All_three_rules_together_reach_the_block_threshold()
    {
        var customer = Guid.CreateVersion7();
        for (var i = 0; i < 5; i++)
            await Handle(TestData.Tx(100m, TestData.Now.AddMinutes(i), customer, country: "DK"));

        // Burst (40) + amount spike (30) + different country minutes later (40) = 110 >= 90.
        var worst = TestData.Tx(1000m, TestData.Now.AddMinutes(5), customer, country: "BR");
        await Handle(worst);

        var stored = await FindTransaction(worst.TransactionId);
        Assert.Equal(Decision.Block, stored?.Decision);
        Assert.Equal(110m, stored?.RiskScore);

        var @case = await FindCase(worst.TransactionId);
        Assert.Equal(Decision.Block, @case?.Decision);
        Assert.Equal(["AMOUNT_SPIKE", "IMPOSSIBLE_TRAVEL", "VELOCITY_HIGH"], @case!.Reasons.Select(r => r.Code).Order());
    }

    [Fact]
    public async Task Transactions_outside_the_window_do_not_count_towards_velocity()
    {
        var customer = Guid.CreateVersion7();
        var scoring = new ScoringOptions { ReviewThreshold = 40 };

        // Six transactions, but an hour apart, never six inside one 10 minute window.
        for (var i = 0; i < 6; i++)
        {
            var tx = TestData.Tx(100m, TestData.Now.AddHours(i), customer);
            await Handle(tx, scoring);
            Assert.Null(await FindCase(tx.TransactionId));
        }
    }
}

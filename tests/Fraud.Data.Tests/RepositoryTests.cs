using Fraud.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class RepositoryTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Transaction_repository_adds_and_finds_a_transaction()
    {
        var transaction = TestData.Transaction();
        await using var db = postgres.CreateContext();
        var repository = new TransactionRepository(NullLogger<TransactionRepository>.Instance, db);
        IUnitOfWork unitOfWork = new UnitOfWork(NullLogger<UnitOfWork>.Instance, db);

        await repository.AddTransactionAsync(transaction);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await repository.GetTransactionByIdAsync(transaction.Id);
        Assert.NotNull(found);
        Assert.Equal(transaction.Amount, found.Amount);
        Assert.Contains(await repository.GetAllTransactionsAsync(), t => t.Id == transaction.Id);
        Assert.Null(await repository.GetTransactionByIdAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Case_repository_adds_and_finds_a_case()
    {
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        await using var db = postgres.CreateContext();
        await new TransactionRepository(NullLogger<TransactionRepository>.Instance, db).AddTransactionAsync(transaction);
        var repository = new CaseRepository(NullLogger<CaseRepository>.Instance, db);
        IUnitOfWork unitOfWork = new UnitOfWork(NullLogger<UnitOfWork>.Instance, db);

        await repository.AddCaseAsync(@case);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await repository.GetCaseByIdAsync(@case.Id);
        Assert.NotNull(found);
        Assert.Equal(transaction.Id, found.TransactionId);
        Assert.Contains(await repository.GetAllCasesAsync(), c => c.Id == @case.Id);
    }

    [Fact]
    public async Task Ground_truth_repository_adds_and_finds_a_label()
    {
        var transactionId = Guid.CreateVersion7();
        await using var db = postgres.CreateContext();
        var repository = new GroundTruthRepository(NullLogger<GroundTruthRepository>.Instance, db);
        IUnitOfWork unitOfWork = new UnitOfWork(NullLogger<UnitOfWork>.Instance, db);

        await repository.AddGroundTruthAsync(transactionId, true, "ImpossibleTravel");
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await repository.GetGroundTruthAsync(transactionId);
        Assert.NotNull(found);
        Assert.True(found.IsFraud);
        Assert.Equal("ImpossibleTravel", found.Pattern);
        Assert.Contains(await repository.GetAllGroundTruthAsync(), g => g.TransactionId == transactionId);
    }

    private static TimeProvider ClockAt(DateTimeOffset now)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        return clock;
    }

    [Fact]
    public async Task Ground_truth_repository_stamps_the_label_with_the_injected_clock()
    {
        var transactionId = Guid.CreateVersion7();
        var written = new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero);
        await using var db = postgres.CreateContext();
        var repository = new GroundTruthRepository(NullLogger<GroundTruthRepository>.Instance, db, ClockAt(written));

        await repository.AddGroundTruthAsync(transactionId, false, "None");
        await new UnitOfWork(NullLogger<UnitOfWork>.Instance, db).SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var read = postgres.CreateContext();
        var found = await read.GroundTruth.AsNoTracking().SingleAsync(g => g.TransactionId == transactionId, TestContext.Current.CancellationToken);
        Assert.Equal(written, found.CreatedAt);
    }

    [Fact]
    public async Task Ground_truth_written_without_a_timestamp_gets_the_database_clock()
    {
        var transactionId = Guid.CreateVersion7();
        var before = DateTimeOffset.UtcNow.AddMinutes(-1);
        await using var db = postgres.CreateContext();

        db.GroundTruth.Add(new Fraud.Data.Entities.GroundTruthEntity { TransactionId = transactionId, IsFraud = true, Pattern = "CardTesting" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var read = postgres.CreateContext();
        var found = await read.GroundTruth.AsNoTracking().SingleAsync(g => g.TransactionId == transactionId, TestContext.Current.CancellationToken);
        Assert.True(found.CreatedAt > before);
    }
}

using Fraud.Data.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

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
}

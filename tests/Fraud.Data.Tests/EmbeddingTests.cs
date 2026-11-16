using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Fraud.Data.Tests;

[Collection(PostgresCollection.Name)]
public class EmbeddingTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Nearest_case_by_cosine_distance_comes_first()
    {
        var ct = TestContext.Current.CancellationToken;
        var near = await AddCaseWithEmbeddingAsync(new Vector(new float[] { 1f, 0f, 0f }), ct);
        var far = await AddCaseWithEmbeddingAsync(new Vector(new float[] { 0f, 1f, 0f }), ct);
        var query = new Vector(new float[] { 0.9f, 0.1f, 0f });

        await using var db = postgres.CreateContext();
        var ordered = await db.Cases
            .Where(c => c.Id == near || c.Id == far)
            .OrderBy(c => c.Embedding!.CosineDistance(query))
            .Select(c => c.Id)
            .ToListAsync(ct);

        Assert.Equal([near, far], ordered);
    }

    private async Task<Guid> AddCaseWithEmbeddingAsync(Vector embedding, CancellationToken ct)
    {
        var transaction = TestData.Transaction();
        var @case = TestData.Case(transaction.Id);
        @case.Embedding = embedding;

        await using var db = postgres.CreateContext();
        db.Transactions.Add(transaction);
        db.Cases.Add(@case);
        await db.SaveChangesAsync(ct);
        return @case.Id;
    }
}

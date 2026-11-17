using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;

namespace Fraud.Scoring.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static TransactionCreated Tx(
        decimal amount = 100m,
        DateTimeOffset? at = null,
        Guid? customerId = null,
        Guid? transactionId = null,
        string country = "DK") =>
        new(transactionId ?? Guid.CreateVersion7(),
            customerId ?? Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "electronics",
            amount,
            "DKK",
            country,
            Channel.Online,
            at ?? Now);

    public static CustomerState State(
        IEnumerable<DateTimeOffset>? recent = null,
        decimal average = 0m,
        int count = 0)
    {
        var times = recent?.ToList() ?? [];
        return new CustomerState(times, "DK", times.Count > 0 ? times.Max() : null, average, count);
    }

    /// <summary>n timestamps, all inside the default 10 minute window before <see cref="Now"/>.</summary>
    public static IEnumerable<DateTimeOffset> RecentTimes(int n) =>
        Enumerable.Range(1, n).Select(i => Now.AddSeconds(-i * 30));

    public static IOptions<ScoringOptions> Options(ScoringOptions? options = null) =>
        Microsoft.Extensions.Options.Options.Create(options ?? new ScoringOptions());
}

internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

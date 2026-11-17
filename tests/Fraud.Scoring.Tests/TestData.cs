using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.State;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fraud.Scoring.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Transactions share one merchant unless a test says otherwise, so they never look like a new-merchant burst.</summary>
    public static readonly Guid UsualMerchant = Guid.CreateVersion7();

    public static TransactionCreated Tx(
        decimal amount = 100m,
        DateTimeOffset? at = null,
        Guid? customerId = null,
        Guid? transactionId = null,
        string country = "DK",
        Guid? merchantId = null) =>
        new(transactionId ?? Guid.CreateVersion7(),
            customerId ?? Guid.CreateVersion7(),
            merchantId ?? UsualMerchant,
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

    public static TimeProvider Clock(DateTimeOffset? now = null)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now ?? Now);
        return clock;
    }

    public static IOptions<ScoringOptions> Options(ScoringOptions? options = null) =>
        Microsoft.Extensions.Options.Options.Create(options ?? new ScoringOptions());
}

using Fraud.Contracts;
using Fraud.Data.Entities;
using Fraud.Data.Repositories;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using NSubstitute;

namespace Fraud.Scoring.Tests.Rules;

public class NewMerchantBurstRuleTests
{
    // Defaults: 4 new merchants within 600 seconds, 40 points, applies from MinHistory (5) transactions.
    private static readonly ScoringOptions Defaults = new();

    private readonly Guid _customer = Guid.CreateVersion7();
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();

    private NewMerchantBurstRule Rule(ScoringOptions? options = null) => new(TestData.Options(options), _repository);

    private CustomerState Established => TestData.State(count: Defaults.MinHistory);

    private TransactionCreated Current(Guid merchant) =>
        TestData.Tx(customerId: _customer, merchantId: merchant, at: TestData.Now);

    private static Guid[] Merchants(int n) =>
        Enumerable.Range(0, n).Select(_ => Guid.CreateVersion7()).ToArray();

    private TransactionEntity Stored(Guid merchant) => new()
    {
        Id = Guid.CreateVersion7(),
        CustomerId = _customer,
        MerchantId = merchant,
        OccurredAt = TestData.Now.AddMinutes(-1),
    };

    // What the repository returns for the burst window, and for everything before it.
    private void StoredTransactions(IEnumerable<Guid> beforeWindow, IEnumerable<Guid> inWindow, ScoringOptions? options = null)
    {
        var windowStart = TestData.Now.AddSeconds(-(options ?? Defaults).NewMerchantBurstWindowSeconds);
        _repository.GetTransactionsByCustomer(_customer, windowStart, TestData.Now).Returns(inWindow.Select(Stored));
        _repository.GetTransactionsByCustomer(_customer, null, windowStart.AddTicks(-1)).Returns(beforeWindow.Select(Stored));
    }

    [Fact]
    public async Task Customer_with_too_little_history_is_skipped_without_querying()
    {
        var state = TestData.State(count: Defaults.MinHistory - 1);

        var hit = await Rule().Evaluate(Current(Guid.CreateVersion7()), state, TestData.Now);

        Assert.Null(hit);
        await _repository.DidNotReceiveWithAnyArgs().GetTransactionsByCustomer(default, default, default);
    }

    [Fact]
    public async Task Threshold_new_merchants_including_the_current_one_hits_with_reason_and_points()
    {
        StoredTransactions(beforeWindow: Merchants(3), inWindow: Merchants(3));

        var hit = await Rule().Evaluate(Current(Guid.CreateVersion7()), Established, TestData.Now);

        Assert.NotNull(hit);
        Assert.Equal("NEW_MERCHANT_BURST", hit.ReasonCode);
        Assert.Equal(Defaults.NewMerchantBurstScore, hit.Points);
    }

    [Fact]
    public async Task One_short_of_the_threshold_does_not_hit()
    {
        StoredTransactions(beforeWindow: Merchants(3), inWindow: Merchants(2));

        var hit = await Rule().Evaluate(Current(Guid.CreateVersion7()), Established, TestData.Now);

        Assert.Null(hit);
    }

    [Fact]
    public async Task Merchants_used_before_the_window_are_not_new()
    {
        var usual = Merchants(4);
        StoredTransactions(beforeWindow: usual, inWindow: usual.Take(3));

        var hit = await Rule().Evaluate(Current(usual[3]), Established, TestData.Now);

        Assert.Null(hit);
    }

    [Fact]
    public async Task The_same_new_merchant_repeated_counts_once()
    {
        var shop = Guid.CreateVersion7();
        StoredTransactions(beforeWindow: Merchants(3), inWindow: [shop, shop, shop]);

        var hit = await Rule().Evaluate(Current(shop), Established, TestData.Now);

        Assert.Null(hit);
    }

    [Fact]
    public async Task Threshold_window_and_points_come_from_options()
    {
        var options = new ScoringOptions
        {
            NewMerchantBurstThreshold = 2,
            NewMerchantBurstWindowSeconds = 60,
            NewMerchantBurstScore = 7,
        };
        StoredTransactions(beforeWindow: Merchants(3), inWindow: Merchants(1), options);

        var hit = await Rule(options).Evaluate(Current(Guid.CreateVersion7()), Established, TestData.Now);

        Assert.Equal(7, hit?.Points);
    }
}

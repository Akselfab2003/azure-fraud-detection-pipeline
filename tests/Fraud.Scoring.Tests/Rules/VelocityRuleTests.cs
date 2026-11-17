using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring.Tests.Rules;

public class VelocityRuleTests
{
    // Defaults: window 10 min, max 5 transactions, 40 points.
    private static readonly ScoringOptions Defaults = new();
    private readonly VelocityRule _rule = new(TestData.Options());

    [Fact]
    public async Task First_ever_transaction_does_not_hit()
    {
        var hit = await _rule.Evaluate(TestData.Tx(), TestData.State(), TestData.Now);

        Assert.Null(hit);
    }

    [Fact]
    public async Task Exactly_at_the_limit_does_not_hit()
    {
        // 4 recent + the current one = 5, and the limit is "more than 5".
        var state = TestData.State(TestData.RecentTimes(Defaults.VelocityMaxCount - 1));

        Assert.Null(await _rule.Evaluate(TestData.Tx(), state, TestData.Now));
    }

    [Fact]
    public async Task One_over_the_limit_hits_with_reason_and_points()
    {
        var state = TestData.State(TestData.RecentTimes(Defaults.VelocityMaxCount));

        var hit = await _rule.Evaluate(TestData.Tx(), state, TestData.Now);

        Assert.NotNull(hit);
        Assert.Equal("VELOCITY_HIGH", hit.ReasonCode);
        Assert.Equal(Defaults.VelocityPoints, hit.Points);
    }

    [Fact]
    public async Task Expired_transactions_are_ignored()
    {
        var old = Enumerable.Range(0, 20).Select(i => TestData.Now.AddMinutes(-30).AddSeconds(i));
        var state = TestData.State(old);

        Assert.Null(await _rule.Evaluate(TestData.Tx(), state, TestData.Now));
    }

    [Fact]
    public async Task A_transaction_exactly_one_window_old_is_expired()
    {
        // The window is exclusive: now - t < window. Five entries exactly 10 minutes old do not count.
        var edge = Enumerable.Repeat(TestData.Now - Defaults.VelocityWindow, Defaults.VelocityMaxCount);

        Assert.Null(await _rule.Evaluate(TestData.Tx(), TestData.State(edge), TestData.Now));
    }

    [Fact]
    public async Task Threshold_comes_from_options()
    {
        var rule = new VelocityRule(TestData.Options(new ScoringOptions { VelocityMaxCount = 1, VelocityPoints = 7 }));
        var state = TestData.State(TestData.RecentTimes(1));

        var hit = await rule.Evaluate(TestData.Tx(), state, TestData.Now);

        Assert.Equal(7, hit?.Points);
    }
}

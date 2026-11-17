using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring.Tests.Rules;

public class AmountBaselineRuleTests
{
    // Defaults: hit when amount > average * 5, 30 points.
    private static readonly ScoringOptions Defaults = new();
    private readonly AmountBaselineRule _rule = new(TestData.Options());

    [Fact]
    public async Task First_ever_transaction_has_no_baseline_and_does_not_hit()
    {
        var hit = await _rule.Evaluate(TestData.Tx(amount: 1_000_000m), TestData.State(), TestData.Now);

        Assert.Null(hit);
    }

    [Fact]
    public async Task Below_minimum_history_does_not_hit()
    {
        var state = TestData.State(average: 100m, count: Defaults.MinHistory - 1);

        Assert.Null(await _rule.Evaluate(TestData.Tx(amount: 10_000m), state, TestData.Now));
    }

    [Fact]
    public async Task Exactly_at_minimum_history_can_hit()
    {
        var state = TestData.State(average: 100m, count: Defaults.MinHistory);

        Assert.NotNull(await _rule.Evaluate(TestData.Tx(amount: 10_000m), state, TestData.Now));
    }

    [Fact]
    public async Task Exactly_on_the_threshold_does_not_hit()
    {
        var state = TestData.State(average: 100m, count: 10);
        var tx = TestData.Tx(amount: 100m * Defaults.AmountMultiplier);

        Assert.Null(await _rule.Evaluate(tx, state, TestData.Now));
    }

    [Fact]
    public async Task Just_above_the_threshold_hits_with_reason_and_points()
    {
        var state = TestData.State(average: 100m, count: 10);
        var tx = TestData.Tx(amount: 100m * Defaults.AmountMultiplier + 0.01m);

        var hit = await _rule.Evaluate(tx, state, TestData.Now);

        Assert.NotNull(hit);
        Assert.Equal("AMOUNT_SPIKE", hit.ReasonCode);
        Assert.Equal(Defaults.AmountPoints, hit.Points);
    }

    [Fact]
    public async Task Normal_amount_does_not_hit()
    {
        var state = TestData.State(average: 100m, count: 10);

        Assert.Null(await _rule.Evaluate(TestData.Tx(amount: 120m), state, TestData.Now));
    }
}

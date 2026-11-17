using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;
using NSubstitute;

namespace Fraud.Scoring.Tests.Rules;

public class RiskScorerTests
{
    // A rule that always returns the given hit (or none).
    private static IRule RuleReturning(RuleHit? hit)
    {
        var rule = Substitute.For<IRule>();
        rule.Evaluate(Arg.Any<TransactionCreated>(), Arg.Any<CustomerState>(), Arg.Any<DateTimeOffset>()).Returns(hit);
        return rule;
    }

    private static RiskScorer Scorer(ScoringOptions options, params RuleHit?[] hits) =>
        new(hits.Select(RuleReturning), TestData.Options(options));

    private static Task<RiskAssessment> Assess(RiskScorer scorer) =>
        scorer.AssessAsync(TestData.State(), TestData.Tx(), TestData.Now);

    [Fact]
    public async Task No_hits_scores_zero_and_allows()
    {
        var result = await Assess(Scorer(new ScoringOptions(), null, null));

        Assert.Equal(0, result.Score);
        Assert.Equal(Decision.Allow, result.Decision);
        Assert.Empty(result.RuleHits);
    }

    [Fact]
    public async Task Score_is_the_sum_of_points_and_keeps_every_hit()
    {
        var result = await Assess(Scorer(new ScoringOptions { ReviewThreshold = 100 },
            new RuleHit("A", 40, "a"), null, new RuleHit("B", 30, "b")));

        Assert.Equal(70, result.Score);
        Assert.Equal(["A", "B"], result.RuleHits.Select(h => h.ReasonCode));
    }

    [Fact]
    public async Task Below_the_threshold_allows()
    {
        var result = await Assess(Scorer(new ScoringOptions { ReviewThreshold = 50 }, new RuleHit("A", 49, "a")));

        Assert.Equal(Decision.Allow, result.Decision);
        Assert.False(result.ShouldReview);
    }

    [Fact]
    public async Task Exactly_on_the_threshold_reviews()
    {
        var result = await Assess(Scorer(new ScoringOptions { ReviewThreshold = 50 }, new RuleHit("A", 50, "a")));

        Assert.Equal(Decision.Review, result.Decision);
        Assert.True(result.ShouldReview);
    }

    [Fact]
    public async Task Exactly_on_the_block_threshold_blocks_and_opens_a_case()
    {
        var options = new ScoringOptions { ReviewThreshold = 50, BlockThreshold = 90 };

        var result = await Assess(Scorer(options, new RuleHit("A", 90, "a")));

        Assert.Equal(Decision.Block, result.Decision);
        Assert.True(result.ShouldReview);
    }

    [Fact]
    public async Task Just_below_the_block_threshold_only_reviews()
    {
        var options = new ScoringOptions { ReviewThreshold = 50, BlockThreshold = 90 };

        var result = await Assess(Scorer(options, new RuleHit("A", 89, "a")));

        Assert.Equal(Decision.Review, result.Decision);
    }

    [Fact]
    public async Task Every_rule_is_evaluated_with_the_transaction_state_and_time()
    {
        var rule = RuleReturning(null);
        var state = TestData.State(count: 3, average: 10m);
        var tx = TestData.Tx();

        await new RiskScorer([rule], TestData.Options()).AssessAsync(state, tx, TestData.Now);

        await rule.Received(1).Evaluate(tx, state, TestData.Now);
    }

    [Fact]
    public async Task Real_rules_combine_to_cross_the_default_threshold()
    {
        var options = TestData.Options();
        var scorer = new RiskScorer([new VelocityRule(options), new AmountBaselineRule(options)], options);
        var state = TestData.State(TestData.RecentTimes(5), average: 100m, count: 10);

        var result = await scorer.AssessAsync(state, TestData.Tx(amount: 1000m), TestData.Now);

        Assert.Equal(70, result.Score);
        Assert.Equal(Decision.Review, result.Decision);
    }
}

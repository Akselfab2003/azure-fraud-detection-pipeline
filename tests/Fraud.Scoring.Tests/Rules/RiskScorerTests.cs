using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;
using Fraud.Scoring.Scoring.State;

namespace Fraud.Scoring.Tests.Rules;

public class RiskScorerTests
{
    private sealed class StubRule(RuleHit? hit) : IRule
    {
        public RuleHit? Evaluate(TransactionCreated transaction, CustomerState state, DateTimeOffset now) => hit;
    }

    private static RiskScorer Scorer(ScoringOptions options, params RuleHit?[] hits) =>
        new(hits.Select(h => new StubRule(h)), TestData.Options(options));

    private static RiskAssessment Assess(RiskScorer scorer) =>
        scorer.Assess(TestData.State(), TestData.Tx(), TestData.Now);

    [Fact]
    public void No_hits_scores_zero_and_allows()
    {
        var result = Assess(Scorer(new ScoringOptions(), null, null));

        Assert.Equal(0, result.Score);
        Assert.Equal(Decision.Allow, result.Decision);
        Assert.Empty(result.RuleHits);
    }

    [Fact]
    public void Score_is_the_sum_of_points_and_keeps_every_hit()
    {
        var result = Assess(Scorer(new ScoringOptions { CaseThreshold = 100 },
            new RuleHit("A", 40, "a"), null, new RuleHit("B", 30, "b")));

        Assert.Equal(70, result.Score);
        Assert.Equal(["A", "B"], result.RuleHits.Select(h => h.ReasonCode));
    }

    [Fact]
    public void Below_the_threshold_allows()
    {
        var result = Assess(Scorer(new ScoringOptions { CaseThreshold = 50 }, new RuleHit("A", 49, "a")));

        Assert.Equal(Decision.Allow, result.Decision);
        Assert.False(result.ShouldReview);
    }

    [Fact]
    public void Exactly_on_the_threshold_reviews()
    {
        var result = Assess(Scorer(new ScoringOptions { CaseThreshold = 50 }, new RuleHit("A", 50, "a")));

        Assert.Equal(Decision.Review, result.Decision);
        Assert.True(result.ShouldReview);
    }

    [Fact]
    public void Real_rules_combine_to_cross_the_default_threshold()
    {
        var options = TestData.Options();
        var scorer = new RiskScorer([new VelocityRule(options), new AmountBaselineRule(options)], options);
        var state = TestData.State(TestData.RecentTimes(5), average: 100m, count: 10);

        var result = scorer.Assess(state, TestData.Tx(amount: 1000m), TestData.Now);

        Assert.Equal(70, result.Score);
        Assert.Equal(Decision.Review, result.Decision);
    }
}

using Fraud.Scoring.Scoring;
using Fraud.Scoring.Scoring.Rules;

namespace Fraud.Scoring.Tests.Rules;

public class ImpossibleTravelRuleTests
{
    // Defaults: different country within 1 hour of the last transaction, 40 points.
    private static readonly ScoringOptions Defaults = new();
    private readonly ImpossibleTravelRule _rule = new(TestData.Options());

    // State whose last transaction was in DK, the given time before Now.
    private static Fraud.Scoring.Scoring.State.CustomerState LastSeenInDk(TimeSpan ago) =>
        TestData.State([TestData.Now - ago], count: 1);

    [Fact]
    public async Task First_ever_transaction_does_not_hit()
    {
        Assert.Null(await _rule.Evaluate(TestData.Tx(country: "BR"), TestData.State(), TestData.Now));
    }

    [Fact]
    public async Task Different_country_inside_the_window_hits_with_reason_and_points()
    {
        var hit = await _rule.Evaluate(TestData.Tx(country: "BR"), LastSeenInDk(TimeSpan.FromMinutes(20)), TestData.Now);

        Assert.NotNull(hit);
        Assert.Equal("IMPOSSIBLE_TRAVEL", hit.ReasonCode);
        Assert.Equal(Defaults.ImpossibleTravelPoints, hit.Points);
    }

    [Fact]
    public async Task Different_country_outside_the_window_does_not_hit()
    {
        Assert.Null(await _rule.Evaluate(TestData.Tx(country: "BR"), LastSeenInDk(TimeSpan.FromHours(5)), TestData.Now));
    }

    [Fact]
    public async Task Different_country_exactly_one_window_later_does_not_hit()
    {
        // The window is exclusive: sinceLast < window.
        Assert.Null(await _rule.Evaluate(TestData.Tx(country: "BR"), LastSeenInDk(Defaults.ImpossibleTravelWindow), TestData.Now));
    }

    [Fact]
    public async Task Same_country_inside_the_window_does_not_hit()
    {
        Assert.Null(await _rule.Evaluate(TestData.Tx(country: "DK"), LastSeenInDk(TimeSpan.FromMinutes(1)), TestData.Now));
    }

    [Fact]
    public async Task Points_and_window_come_from_options()
    {
        var rule = new ImpossibleTravelRule(TestData.Options(new ScoringOptions
        {
            ImpossibleTravelWindow = TimeSpan.FromHours(6),
            ImpossibleTravelPoints = 9,
        }));

        var hit = await rule.Evaluate(TestData.Tx(country: "BR"), LastSeenInDk(TimeSpan.FromHours(5)), TestData.Now);

        Assert.Equal(9, hit?.Points);
    }
}

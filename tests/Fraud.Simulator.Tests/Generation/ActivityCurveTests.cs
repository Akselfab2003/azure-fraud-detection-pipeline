using Fraud.Simulator.Generation;

namespace Fraud.Simulator.Tests.Generation
{
    public class ActivityCurveTests
    {
        private static readonly DateTimeOffset Midnight = new(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Weights_ShouldAverageOneOverADay()
        {
            var curve = new ActivityCurve();

            var mean = Enumerable.Range(0, 24).Average(h => curve.Weight(Midnight.AddHours(h)));

            Assert.Equal(1.0, mean, precision: 9);
        }

        [Fact]
        public void MaxWeight_ShouldBeTheHighestHourlyWeight()
        {
            var curve = new ActivityCurve();

            var highest = Enumerable.Range(0, 24).Max(h => curve.Weight(Midnight.AddHours(h)));

            Assert.Equal(highest, curve.MaxWeight, precision: 9);
        }

        [Fact]
        public void Night_ShouldBeQuieterThanEvening()
        {
            var curve = new ActivityCurve();

            Assert.True(curve.Weight(Midnight.AddHours(3)) < curve.Weight(Midnight.AddHours(18)));
        }
    }
}

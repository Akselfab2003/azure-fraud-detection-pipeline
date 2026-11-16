namespace Fraud.Simulator.Generation
{
    public sealed class ActivityCurve
    {
        private static readonly double[] Hourly =
            [0.2, 0.1, 0.1, 0.1, 0.1, 0.3, 0.6, 1.0, 1.2, 1.2, 1.3, 1.6, 1.8, 1.5, 1.3, 1.3, 1.5, 1.8, 2.0, 1.8, 1.4, 1.0, 0.6, 0.3];

        public double Weight(DateTimeOffset t) => Hourly[t.Hour] / Hourly.Average();
    }
}

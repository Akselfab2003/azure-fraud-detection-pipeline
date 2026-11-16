namespace Fraud.Simulator.Tests
{
    /// <summary>A clock that always returns the same instant, so generated timestamps are reproducible.</summary>
    internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

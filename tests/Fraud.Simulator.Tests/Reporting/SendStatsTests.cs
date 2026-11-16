using Fraud.Contracts;
using Fraud.Contracts.Enums;
using Fraud.Simulator.Generation;
using Fraud.Simulator.Patterns;
using Fraud.Simulator.Reporting;

namespace Fraud.Simulator.Tests.Reporting
{
    public class SendStatsTests
    {
        private static GeneratedTransaction Tx(FraudPattern pattern)
            => new(new TransactionCreated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Grocery", 10m, "DKK", "DK",
                                          Channel.CardPresent, DateTimeOffset.UnixEpoch),
                   pattern);

        [Fact]
        public void Drain_ShouldSummariseAndThenReset()
        {
            var stats = new SendStats();
            stats.Record(Tx(FraudPattern.None));
            stats.Record(Tx(FraudPattern.None));
            stats.Record(Tx(FraudPattern.CardTesting));
            stats.Record(Tx(FraudPattern.CardTesting));
            stats.Record(Tx(FraudPattern.ImpossibleTravel));

            var summary = stats.Drain();

            Assert.Equal(5, summary.Sent);
            Assert.Equal(3, summary.Fraud);
            Assert.Equal(2, summary.ByPattern[FraudPattern.CardTesting]);
            Assert.Equal(1, summary.ByPattern[FraudPattern.ImpossibleTravel]);
            Assert.DoesNotContain(FraudPattern.None, summary.ByPattern.Keys);

            var empty = stats.Drain();
            Assert.Equal(0, empty.Sent);
            Assert.Equal(0, empty.Fraud);
            Assert.Empty(empty.ByPattern);
        }
    }
}

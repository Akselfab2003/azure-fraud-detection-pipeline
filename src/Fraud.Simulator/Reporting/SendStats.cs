using Fraud.Simulator.Generation;
using Fraud.Simulator.Patterns;

namespace Fraud.Simulator.Reporting
{
    public sealed record SendSummary(int Sent, int Fraud, IReadOnlyDictionary<FraudPattern, int> ByPattern);

    /// <summary>Counts what the worker has sent since the last <see cref="Drain"/>.</summary>
    public sealed class SendStats
    {
        private int _sent;
        private int _fraud;
        private readonly Dictionary<FraudPattern, int> _byPattern = [];

        public void Record(GeneratedTransaction tx)
        {
            _sent++;
            if (!tx.IsFraud)
                return;

            _fraud++;
            _byPattern[tx.Pattern] = _byPattern.GetValueOrDefault(tx.Pattern) + 1;
        }

        public SendSummary Drain()
        {
            var summary = new SendSummary(_sent, _fraud, new Dictionary<FraudPattern, int>(_byPattern));
            _sent = 0;
            _fraud = 0;
            _byPattern.Clear();
            return summary;
        }
    }
}

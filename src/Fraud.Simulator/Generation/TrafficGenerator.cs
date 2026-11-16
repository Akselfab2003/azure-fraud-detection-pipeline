using Fraud.Simulator.Generator;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Generation
{
    public sealed class TrafficGenerator
    {
        private readonly PriorityQueue<(CustomerProfile C, bool IsFraud, GeneratedTransaction? Tx), DateTimeOffset> _queue = new();

        public IEnumerable<GeneratedTransaction> Stream()
        {
            SeedQueue();
            long total = 0, fraud = 0;
            while (true)
            {
                _queue.TryDequeue(out var item, out var priority);

                if (item.Tx is { } ready)
                { fraud++; total++; yield return ready; continue; }


            }
        }
    }
}

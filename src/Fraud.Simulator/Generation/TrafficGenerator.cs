using Fraud.Simulator.Configuration;
using Fraud.Simulator.Patterns;
using Fraud.Simulator.Population;

namespace Fraud.Simulator.Generation
{
    public sealed class TrafficGenerator
    {
        private readonly record struct Scheduled(CustomerProfile? Customer, GeneratedTransaction? Ready);
        private readonly PriorityQueue<Scheduled, (DateTimeOffset At, long Seq)> _queue = new();
        private readonly Population.Population _population;
        private readonly NormalTraffic _normal;
        private readonly IReadOnlyList<IFraudPattern> _patterns;
        private readonly double _fraudRatio;
        private readonly Random _normalRng;
        private readonly Random _fraudRng;
        private readonly DateTimeOffset _start;
        private long _seq;

        public TrafficGenerator(Population.Population population, NormalTraffic normal,
            IEnumerable<IFraudPattern> allPatterns, SimulatorOptions options, TimeProvider time)
        {
            if (options.FraudShare is < 0 or >= 1)
                throw new ArgumentOutOfRangeException(nameof(options), "FraudShare must be in [0, 1).");

            _population = population;
            _normal = normal;
            _patterns = allPatterns.Where(p => options.EnabledPatterns.Contains(p.Kind)).ToList();
            _fraudRatio = options.FraudShare / (1 - options.FraudShare);
            _normalRng = new Random(options.Seed);
            _fraudRng = new Random(unchecked(options.Seed * 31 + 1));   // separate stream
            _start = time.GetUtcNow();
        }

        public IEnumerable<GeneratedTransaction> Stream()
        {
            foreach (var c in _population.Customers)
                Enqueue(new(c, null), _normal.NextTime(c, _start, _normalRng));

            long normalCount = 0, fraudPlanned = 0;

            while (_queue.TryDequeue(out var item, out var key))
            {
                if (item.Ready is { } fraudTx)              // pre-built fraud transaction
                {
                    yield return fraudTx;
                    continue;
                }

                var customer = item.Customer!;
                normalCount++;
                yield return _normal.Create(customer, key.At, _normalRng);
                Enqueue(new(customer, null), _normal.NextTime(customer, key.At, _normalRng));

                if (_patterns.Count > 0 && fraudPlanned < _fraudRatio * normalCount)
                    fraudPlanned += InjectFraud(key.At);
            }
        }

        private int InjectFraud(DateTimeOffset now)
        {
            var pattern = _patterns[_fraudRng.Next(_patterns.Count)];
            var victim = _population.Customers[_fraudRng.Next(_population.Customers.Count)];
            var burst = pattern.Generate(victim, _population, now, _fraudRng);

            foreach (var tx in burst)
            {
                if (tx.Message.OccurredAt < now)
                    throw new InvalidOperationException($"{pattern.Kind} produced a transaction before its start time.");
                Enqueue(new(null, tx), tx.Message.OccurredAt);
            }
            // Patterns may add legitimate "anchor" transactions (label None); only labelled fraud counts toward the share.
            return burst.Count(t => t.IsFraud);
        }

        private void Enqueue(Scheduled s, DateTimeOffset at) => _queue.Enqueue(s, (at, _seq++));
    }
}

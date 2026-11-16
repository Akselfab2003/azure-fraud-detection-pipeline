using Fraud.Simulator.Patterns;

namespace Fraud.Simulator.Configuration
{
    public sealed class SimulatorOptions
    {
        public int Seed { get; set; } = 42;
        public int CustomerCount { get; set; } = 1_000;
        public int MerchantCount { get; set; } = 200;

        /// <summary>
        /// Gets or sets the target requests per second.
        /// </summary>
        public double RatePerSecond { get; set; } = 1;
        public double FraudShare { get; set; } = 0.02;
        public FraudPattern[] EnabledPatterns { get; set; } =
        [
            FraudPattern.CardTesting,
            FraudPattern.AccountTakeover,
            FraudPattern.ImpossibleTravel,
            FraudPattern.NewMerchantBurst
        ];
    }
}

namespace Fraud.Scoring.Scoring
{
    public sealed class ScoringOptions
    {
        public const string SectionName = "Scoring";

        /// <summary>Stored on every case, so you can tell which rules and thresholds made a decision.</summary>
        public string RuleSetVersion { get; init; } = "rules-v1";

        /// <summary>A risk score at or above this opens a case (Decision.Review).</summary>
        public decimal ReviewThreshold { get; init; } = 40m;

        /// <summary>A risk score at or above this is Decision.Block. Keep it above ReviewThreshold.</summary>
        public decimal BlockThreshold { get; init; } = 90m;

        public TimeSpan VelocityWindow { get; init; } = TimeSpan.FromMinutes(10);
        public int VelocityMaxCount { get; init; } = 5;
        public int VelocityPoints { get; init; } = 40;

        public decimal AmountMultiplier { get; init; } = 5m;
        public int AmountPoints { get; init; } = 30;
        /// <summary>The amount rule only applies once the customer has this many transactions, so a tiny history can't set the baseline.</summary>
        public int MinHistory { get; init; } = 5;

        /// <summary>A different country within this long of the last transaction counts as impossible travel.</summary>
        public TimeSpan ImpossibleTravelWindow { get; init; } = TimeSpan.FromHours(1);
        public int ImpossibleTravelPoints { get; init; } = 40;



        public int NewMerchantBurstThreshold { get; init; } = 4;
        public int NewMerchantBurstWindowSeconds { get; init; } = 600;
        public int NewMerchantBurstScore { get; init; } = 40;

    }
}

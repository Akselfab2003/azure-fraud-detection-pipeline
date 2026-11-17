namespace Fraud.Scoring.Scoring
{
    public sealed class ScoringOptions
    {
        public const string SectionName = "Scoring";
        public string RulesCheckVersion { get; init; } = "1.0";
        public int CaseThreshold { get; init; } = 50;
        public TimeSpan VelocityWindow { get; init; } = TimeSpan.FromMinutes(10);
        public int VelocityMaxTransactions { get; init; } = 5;
        public int VelocityPoints { get; init; } = 40;
        public decimal AmountMultiplier { get; init; } = 5m;
        public int AmountPoints { get; init; } = 30;
    }
}

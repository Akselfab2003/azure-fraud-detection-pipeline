namespace Fraud.Scoring.Scoring.State
{
    public sealed record CustomerState
    {
        public CustomerState(IReadOnlyList<DateTimeOffset> recentTransactionTimes,
                             string? lastCountry,
                             DateTimeOffset? lastTransactionAt,
                             decimal averageAmount,
                             int transactionCount)
        {
            RecentTransactionTimes = recentTransactionTimes;
            LastCountry = lastCountry;
            LastTransactionAt = lastTransactionAt;
            AverageAmount = averageAmount;
            TransactionCount = transactionCount;
        }

        public IReadOnlyList<DateTimeOffset> RecentTransactionTimes { get; set; }
        public string? LastCountry { get; set; }
        public DateTimeOffset? LastTransactionAt { get; set; }
        public decimal AverageAmount { get; set; }
        public int TransactionCount { get; set; }

        public static CustomerState EmptyState => new CustomerState(Array.Empty<DateTimeOffset>(), null, null, 0m, 0);
    }
}

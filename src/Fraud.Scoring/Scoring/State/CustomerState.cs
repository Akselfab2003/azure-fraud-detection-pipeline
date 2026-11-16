namespace Fraud.Scoring.Scoring.State
{
    public sealed record CustomerState(IReadOnlyList<DateTimeOffset> RecentTransactionTimes,
                                       string? LastCountry,
                                       DateTimeOffset? LastTransactionAt,
                                       decimal AverageAmount,
                                       int TransactionCount);
}

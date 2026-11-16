using Fraud.Contracts.Enums;

namespace Fraud.Contracts
{
    public sealed record TransactionCreated(
        Guid TransactionId,
    Guid CustomerId,
    Guid MerchantId,
    string MerchantCategory,
    decimal Amount,
    string Currency,             // "DKK"
    string Country,              // "DK"
    Channel Channel,
    DateTimeOffset OccurredAt,
    int SchemaVersion = 1);
}

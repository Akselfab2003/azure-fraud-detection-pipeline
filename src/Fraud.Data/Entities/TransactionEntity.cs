
using Fraud.Contracts.Enums;

namespace Fraud.Data.Entities
{
    public class TransactionEntity
    {
        public Guid Id { get; set; }                  // = TransactionId, unique -> idempotency
        public Guid CustomerId { get; set; }
        public Guid MerchantId { get; set; }
        public string MerchantCategory { get; set; } = "";
        public decimal Amount { get; set; }           // numeric(18,2)
        public string Currency { get; set; } = "";    // char(3)
        public string Country { get; set; } = "";     // char(2)
        public Channel Channel { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public decimal RiskScore { get; set; }
        public Decision Decision { get; set; }
        public DateTimeOffset ScoredAt { get; set; }
    }

}

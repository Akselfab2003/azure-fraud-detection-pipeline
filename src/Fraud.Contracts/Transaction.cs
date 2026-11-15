namespace Fraud.Contracts
{
    public sealed record Transaction
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Guid MerchantId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFraud { get; set; }
    }
}

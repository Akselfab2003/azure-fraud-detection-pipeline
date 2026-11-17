namespace Fraud.Data.Entities
{
    // Table "ground_truth": written by the Simulator only
    public class GroundTruthEntity
    {
        public Guid TransactionId { get; set; }       // PK
        public bool IsFraud { get; set; }
        public string Pattern { get; set; } = "None"; // string: Data must not reference the Simulator
    }
}

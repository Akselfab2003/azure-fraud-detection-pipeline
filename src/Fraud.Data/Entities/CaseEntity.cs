using Fraud.Contracts.Enums;
using Fraud.Data.Enums;
using Pgvector;

namespace Fraud.Data.Entities
{
    public class CaseEntity
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }       // unique FK
        public TransactionEntity Transaction { get; set; } = null!;
        public decimal RiskScore { get; set; }
        public Decision Decision { get; set; }
        public List<CaseReason> Reasons { get; set; } = [];
        public double? AnomalyScore { get; set; }
        public string RuleSetVersion { get; set; } = "";
        public string? ModelVersion { get; set; }
        public CaseStatus Status { get; set; } = CaseStatus.Open;
        public CaseLabel? Label { get; set; }
        public string? LabelledBy { get; set; }
        public DateTimeOffset? LabelledAt { get; set; }
        public string? LabelNote { get; set; }
        public string? Explanation { get; set; }      // step 3.5
        public string? PromptVersion { get; set; }
        public Vector? Embedding { get; set; }        // step 3.4, Pgvector.Vector
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public uint Version { get; set; }             // mapped to xmin
        public List<CaseEventEntity> Events { get; set; } = [];
    }

}

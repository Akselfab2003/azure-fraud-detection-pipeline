using Fraud.Data.Enums;

namespace Fraud.Data.Entities
{

    // Table "case_events": append-only audit trail
    public class CaseEventEntity
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public CaseEventType Type { get; set; }
        public string Actor { get; set; } = "";       // "scoring", "explainer", or the analyst's user id
        public string? DataJson { get; set; }         // jsonb, e.g. old/new status
        public DateTimeOffset At { get; set; }
    }
}

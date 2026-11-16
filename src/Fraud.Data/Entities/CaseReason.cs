namespace Fraud.Data.Entities
{
    // Stored as a jsonb list on the case
    public sealed record CaseReason(string Code, int Points, string Detail);
}

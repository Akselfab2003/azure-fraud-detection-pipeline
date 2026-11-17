namespace Fraud.Scoring.Scoring.State
{
    public interface ICustomerStateStore
    {
        Task<CustomerState?> GetCustomerState(Guid CustomerId, CancellationToken ct);
        Task UpsertCustomerState(Guid CustomerId, CustomerState customerState, CancellationToken ct);
    }
}

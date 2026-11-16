namespace Fraud.Simulator.DataModels
{
    public sealed record Merchant(Guid Id, string Name, string Category, string Country);
}

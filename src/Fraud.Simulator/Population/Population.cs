namespace Fraud.Simulator.Population
{
    public sealed record Population(IReadOnlyList<CustomerProfile> Customers, IReadOnlyList<Merchant> Merchants)
    {
        public IReadOnlyDictionary<Guid, Merchant> MerchantsById { get; } = Merchants.ToDictionary(m => m.Id);
        public ILookup<string, Merchant> MerchantsByCountry { get; } = Merchants.ToLookup(m => m.Country);
    }
}

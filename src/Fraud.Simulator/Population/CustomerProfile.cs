namespace Fraud.Simulator.Population
{
    public sealed record CustomerProfile(Guid Id,
                                         string HomeCountry,
                                         decimal TypicalAmount,
                                         decimal AmountSpread,
                                         double TransactionsPerDay,
                                         IReadOnlyList<Guid> UsualMerchantIds);
}

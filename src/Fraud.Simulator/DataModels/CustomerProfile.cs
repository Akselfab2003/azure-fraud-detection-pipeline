namespace Fraud.Simulator.DataModels
{
    public sealed record CustomerProfile(Guid Id,
                                         string HomeCountry,
                                         decimal TypicalAmount,
                                         decimal AmountSpread,
                                         double TransactionsPerDay,
                                         IReadOnlyList<Guid> UsualMerchantIds);
}

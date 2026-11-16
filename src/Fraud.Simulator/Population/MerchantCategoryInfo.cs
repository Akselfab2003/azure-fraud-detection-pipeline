using Fraud.Contracts.Enums;

namespace Fraud.Simulator.Population
{
    public sealed record MerchantCategoryInfo(
    string Name,              // goes into TransactionCreated.MerchantCategory
    double Weight,            // share of merchants (and of normal traffic)
    double AmountMultiplier,  // scales the customer's TypicalAmount
    Channel[] Channels,       // plausible channels, first is the most common
    bool FraudTarget);        // favoured by takeover / burst patterns
}

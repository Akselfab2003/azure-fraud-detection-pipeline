using Fraud.Contracts.Enums;

namespace Fraud.Simulator.Population
{
    public static class MerchantCatalog
    {
        public static readonly IReadOnlyList<MerchantCategoryInfo> Categories =
        [
            new("Grocery",        0.28, 0.4,  [Channel.CardPresent],                  false),
            new("Restaurant",     0.14, 0.6,  [Channel.CardPresent],                  false),
            new("Fuel",           0.07, 0.8,  [Channel.CardPresent],                  false),
            new("Transport",      0.07, 0.3,  [Channel.CardPresent, Channel.Online],  false),
            new("Retail",         0.12, 1.0,  [Channel.CardPresent, Channel.Online],  false),
            new("Utilities",      0.06, 2.5,  [Channel.Online],                       false),
            new("Streaming",      0.05, 0.15, [Channel.Online],                       false),
            new("Pharmacy",       0.05, 0.5,  [Channel.CardPresent],                  false),
            new("Travel",         0.04, 4.0,  [Channel.Online],                       true),
            new("Electronics",    0.04, 3.5,  [Channel.Online, Channel.CardPresent],  true),
            new("Jewelry",        0.01, 6.0,  [Channel.CardPresent, Channel.Online],  true),
            new("GiftCards",      0.02, 1.5,  [Channel.Online],                       true),
            new("Gaming",         0.03, 0.4,  [Channel.Online],                       true),
            new("Charity",        0.01, 0.3,  [Channel.Online],                       true),  // card-testing favourite
            new("CashWithdrawal", 0.01, 2.0,  [Channel.Atm],                          false),
        ];


        private static IReadOnlyDictionary<string, MerchantCategoryInfo> ByName = Categories.ToDictionary(c => c.Name);

        public static MerchantCategoryInfo Get(string name) => ByName[name];

        public static string PickCategory(Random rng)
        {
            var r = rng.NextDouble();
            var cumulative = 0.0;
            foreach (var category in Categories)
            {
                cumulative += category.Weight;
                if (r < cumulative)
                    return category.Name;
            }
            throw new InvalidOperationException("Failed to pick a merchant category");
        }
    }
}

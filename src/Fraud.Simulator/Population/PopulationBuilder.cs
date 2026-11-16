using Bogus;
using Fraud.Simulator.Configuration;

namespace Fraud.Simulator.Population
{
    public sealed class PopulationBuilder(SimulatorOptions options)
    {
        private static readonly (string Country, double Weight)[] HomeCountries
            = [("DK", 0.85), ("SE", 0.05), ("NO", 0.05), ("DE", 0.05)];

        public Population Build()
        {
            var merchants = BuildMerchants();
            var customers = Enumerable.Range(0, options.CustomerCount)
                                      .Select(i => BuildCustomer(i, merchants))
                                      .ToList();

            return new Population(customers, merchants);
        }

        private List<Merchant> BuildMerchants()
        {
            var faker = new Faker("en") { Random = new Randomizer(options.Seed) };
            var rng = new Random(options.Seed);
            return [.. Enumerable.Range(0, options.MerchantCount).Select(_ =>
            {
                var category = MerchantCatalog.PickCategory(rng);
                var country = Pick(HomeCountries, rng);
                return new Merchant(SeededGuid(rng), faker.Company.CompanyName(), category, country);
            })];
        }


        private CustomerProfile BuildCustomer(int index, IReadOnlyList<Merchant> merchants)
        {
            var rng = new Random(options.Seed * 31 + index);
            var home = Pick(HomeCountries, rng);
            var typical = Math.Round((decimal)LogNormal(rng: rng, median: 250, sigma: 0.6), 2);

            var local = merchants.Where(m => m.Country == home).ToList();
            var pool = local.Count >= 8 ? local : merchants; // if there are not enough local merchants, use the full pool
            var usual = pool.OrderBy(_ => rng.Next()).Take(rng.Next(5, 16)).Select(m => m.Id).ToList();

            return new CustomerProfile(Id: SeededGuid(rng),
                                       HomeCountry: home,
                                       TypicalAmount: typical,
                                       AmountSpread: Math.Round((decimal)(0.3 + rng.NextDouble() * 0.4), 2),
                                       TransactionsPerDay: Math.Clamp(LogNormal(rng: rng, median: 1.5, sigma: 0.5), 0.2, 8),
                                       UsualMerchantIds: usual);
        }

        // This method generates a seeded Guid based on the provided Random instance. It creates a 16-byte array, fills it with random bytes, and constructs a Guid from those bytes.
        // this is required to ensure that the same Random instance produces the same sequence of Guids, which is important for reproducibility in simulations.
        private static Guid SeededGuid(Random rng)
        {
            Span<byte> b = stackalloc byte[16];
            rng.NextBytes(b);
            return new Guid(b);
        }

        private static double LogNormal(Random rng, double median, double sigma)
        {
            // Use the Box-Muller transform to generate a normally distributed random number
            var u1 = 1 - rng.NextDouble();
            var u2 = rng.NextDouble();
            var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return median * Math.Exp(sigma * z);
        }


        private static T Pick<T>(IReadOnlyList<(T Value, double Weight)> items, Random rng)
        {
            var r = rng.NextDouble();
            var cumulative = 0.0;
            foreach (var item in items)
            {
                cumulative += item.Weight;
                if (r < cumulative)
                    return item.Value;
            }
            throw new InvalidOperationException("Failed to pick an item");
        }
    }
}

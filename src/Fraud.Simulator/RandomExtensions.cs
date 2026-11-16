namespace Fraud.Simulator
{
    public static class RandomExtensions
    {
        // This method generates a seeded Guid based on the provided Random instance. It creates a 16-byte array, fills it with random bytes, and constructs a Guid from those bytes.
        // this is required to ensure that the same Random instance produces the same sequence of Guids, which is important for reproducibility in simulations.
        public static Guid SeededGuid(Random rng)
        {
            Span<byte> b = stackalloc byte[16];
            rng.NextBytes(b);
            return new Guid(b);
        }

        public static double LogNormal(Random rng, double median, double sigma)
        {
            // Use the Box-Muller transform to generate a normally distributed random number
            var u1 = 1 - rng.NextDouble();
            var u2 = rng.NextDouble();
            var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return median * Math.Exp(sigma * z);
        }
    }
}

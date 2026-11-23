using System;
using System.Globalization;

namespace Scripts.Services.Seed
{
    // Separate streams so a reroll in one item screen never shifts later items or floor layouts.
    public enum SeedStream
    {
        FloorLayout = 1,
        ItemScreenChance = 2,
        ItemOffer = 3,
        ExtraPick = 4,
    }

    public static class SeedUtility
    {
        private const int FormattedLength = 8;

        public static int Combine(int seed, SeedStream stream, params int[] values)
        {
            unchecked
            {
                uint hash = Mix((uint)seed ^ 0x9E3779B9u);
                hash = Mix(hash ^ (uint)stream);
                if (values != null)
                {
                    foreach (var value in values)
                        hash = Mix(hash ^ (uint)value);
                }

                return (int)hash;
            }
        }

        public static Random CreateRandom(int seed, SeedStream stream, params int[] values) =>
            new Random(Combine(seed, stream, values));

        public static int CreateRandomSeed() =>
            Guid.NewGuid().GetHashCode();

        public static string Format(int seed) =>
            ((uint)seed).ToString("X" + FormattedLength, CultureInfo.InvariantCulture);

        // Hex seeds parse directly; any other text (e.g. "goose") is hashed.
        public static bool TryParse(string text, out int seed)
        {
            seed = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim().Replace(" ", string.Empty);
            if (text.StartsWith("#")) text = text.Substring(1);
            if (text.Length == 0) return false;

            if (text.Length <= FormattedLength &&
                uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
            {
                seed = unchecked((int)hex);
                return true;
            }

            seed = HashString(text);
            return true;
        }

        // FNV-1a: stable across runtimes, unlike string.GetHashCode().
        private static int HashString(string text)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in text.ToUpperInvariant())
                {
                    hash ^= c;
                    hash *= 16777619u;
                }

                return (int)Mix(hash);
            }
        }

        // murmur3 finalizer
        private static uint Mix(uint h)
        {
            unchecked
            {
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return h;
            }
        }
    }
}

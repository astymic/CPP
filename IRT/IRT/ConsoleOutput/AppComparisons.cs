using IRT.Interfaces;
using System;

namespace IRT.ConsoleOutput
{
    public static class AppComparisons
    {
        public static Comparison<IName> NameComparison { get; } = (x, y) =>
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        };

        public static Comparison<IName> PriceComparison { get; } = (x, y) =>
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Price.CompareTo(y.Price);
        };
    }
}
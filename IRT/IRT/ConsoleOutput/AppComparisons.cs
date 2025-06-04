using IRT.Interfaces;
using System;

namespace IRT.ConsoleOutput
{
    public static class AppComparisons
    {
        public class NameComparer<T> : IComparer<T> where T : IName
        {
            public int Compare(T? x, T? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        public class PriceComparer<T> : IComparer<T> where T : IPrice
        {
            public int Compare(T? x, T? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Price.CompareTo(y.Price);
            }
        }
    }
}
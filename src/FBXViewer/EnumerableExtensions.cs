using System;
using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public static class EnumerableExtensions
    {
        public static bool In<T>(this T self, params T[] items)
        {
            return items.Any(i => i?.Equals(self) ?? false);
        }

        public static bool NotIn<T>(this T self, params T[] items)
        {
            return !(self.In(items));
        }
        public static IEnumerable<(T First, T Last)> Pairs<T>(this IEnumerable<T> self)
        {
            using (var enumerator = self.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("Needs at least two items");
                }

                var first = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("Needs at least two items");
                }

                var second = enumerator.Current;
                yield return (first, second);

                var previous = second;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    yield return (previous, current);
                    previous = current;
                }

            }
        }
    }
}
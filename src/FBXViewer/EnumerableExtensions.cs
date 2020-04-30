using System;
using System.Collections.Generic;

namespace FBXViewer
{
    public static class EnumerableExtensions
    {
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
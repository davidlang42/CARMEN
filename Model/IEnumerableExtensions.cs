using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Model
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Cast<int>().Sum();

        public static double Product(this IEnumerable<double> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("Cannot calculate product of an empty sequence.");
            var result = e.Current;
            while (e.MoveNext())
                result *= e.Current;
            return result;
        }

        /// <summary>Similar to SingleOrDefault(), except if the sequence contains more than 1 element,
        /// the default is returned rather than throwing an exception.</summary>
        public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                return default;
            var first = e.Current;
            if (e.MoveNext())
                return default;
            return first;
        }
    }
}

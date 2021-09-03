using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    internal static class Extensions
    {
        public static double Mean(this IEnumerable<uint> values, out uint min, out uint max, out double median)
        {
            var list = new List<uint>();
            uint total;
            var e = values.GetEnumerator();
            if (!e.MoveNext())
                return median = min = max = 0; // empty sequence
            list.Add(e.Current);
            total = min = max = e.Current;
            while (e.MoveNext())
            {
                list.Add(e.Current);
                total += e.Current;
                if (e.Current < min)
                    min = e.Current;
                if (e.Current > max)
                    max = e.Current;
            }
            var middle = list.Count / 2;
            if (list.Count % 2 == 0)
                median = (list[middle] + list[middle - 1]) / 2.0;
            else
                median = list[middle];
            return (double)total / list.Count;
        }
    }

    internal static class IEnumerableExtensions
    {
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

        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> collection)
        {
            foreach (var item in collection)
                set.Add(item);
        }    
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    internal static class Extensions
    {
        /// <summary>Calculates the min/max/mean/middle of a set of values
        /// NOTE: median will only be correct only if values are in order</summary>
        public static double Mean(this IEnumerable<uint> values, out uint min, out uint max, out double median_if_in_order)
        {
            var list = new List<uint>();
            uint total;
            var e = values.GetEnumerator();
            if (!e.MoveNext())
                return median_if_in_order = min = max = 0; // empty sequence
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
                median_if_in_order = (list[middle] + list[middle - 1]) / 2.0;
            else
                median_if_in_order = list[middle];
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
        
        public static T? FindAndRemove<T>(this Stack<T> stack, Predicate<T> predicate)
            where T : class
        {
            var skipped = new Stack<T>();
            T? found = null;
            while (stack.Count > 0)
            {
                var next = stack.Pop();
                if (predicate(next))
                {
                    found = next;
                    break;
                }
                skipped.Push(next);
            }
            while (skipped.Count > 0)
                stack.Push(skipped.Pop());
            return found;
        }
    }
}

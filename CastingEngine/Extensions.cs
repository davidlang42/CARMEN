using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
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
}

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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }
    }
}

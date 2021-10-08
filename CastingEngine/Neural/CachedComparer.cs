using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// An IComparer<typeparamref name="T"/> which caches the results of calling the provided comparer.
    /// NOTE: The comparison of null objects is not allowed
    /// </summary>
    public class CachedComparer<T> : IComparer<T>
        where T : notnull
    {
        IComparer<T> comparer;
        Dictionary<(T, T), int> comparisonCache = new();

        public CachedComparer(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(T? a, T? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            if (comparisonCache.TryGetValue((a, b), out var cached_result))
                return cached_result;
            var result = comparer.Compare(a, b);
            comparisonCache.Add((a, b), result);
            return result;
        }
    }
}

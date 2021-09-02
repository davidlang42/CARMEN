using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// An object which has a name
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }

    public interface INameOrdered : INamed, IComparable
    {
        int IComparable.CompareTo(object? obj) => Name.CompareTo(((INamed?)obj)?.Name);
    }

    public static class INameOrderedExtensions
    {
        /// <summary>Enumerates this queryable ordered by Name value.</summary>
        public static IOrderedQueryable<T> InNameOrder<T>(this IQueryable<T> objects) where T : INameOrdered
            => objects.OrderBy(o => o.Name);

        /// <summary>Enumerates this collection ordered by Name value.</summary>
        public static IOrderedEnumerable<T> InNameOrder<T>(this IEnumerable<T> objects) where T : INameOrdered
            => objects.OrderBy(o => o.Name);
    }
}

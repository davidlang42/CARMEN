using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    /// <summary>
    /// An object which can be ordered when in a collection
    /// </summary>
    public interface IOrdered
    {
        int Order { get; }
    }

    public static class IOrderedExtensions
    {
        public static IEnumerable<T> InOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.OrderBy(o => o.Order);

        public static int NextOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.Any() ? objects.Select(o => o.Order).Max() + 1 : 0;

        //TODO helpers to set order as well
    }
}

using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.Converters
{
    /// <summary>
    /// A static class of standard SortDescriptions for ViewSources
    /// </summary>
    public static class StandardSort
    {
        private static readonly SortDescription IOrdered = new(nameof(Carmen.ShowModel.IOrdered.Order), ListSortDirection.Ascending);
        private static readonly SortDescription INameOrdered = new(nameof(Carmen.ShowModel.INameOrdered.Name), ListSortDirection.Ascending);

        public static SortDescription For<T>(T? _ = null) where T : class => For(typeof(T));
        public static SortDescription For<T>(IEnumerable<T> _) where T : class => For(typeof(T));

        private static SortDescription For(Type type)
        {
            if (type.IsAssignableTo(typeof(IOrdered)))
                return IOrdered;
            else if (type.IsAssignableTo(typeof(INameOrdered)))
                return INameOrdered;
            else
                throw new ArgumentException($"No standard sort found for {type.Name}.");
        }
    }
}

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
        public static readonly SortDescription IOrdered = new(nameof(ShowModel.IOrdered.Order), ListSortDirection.Ascending);
        public static readonly SortDescription INamed = new(nameof(ShowModel.INamed.Name), ListSortDirection.Ascending);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class Column<T>
    {
        public string Name { get; }
        public Func<T, object?> ValueGetter { get; }
        public bool Show { get; set; } = true;
        public ListSortDirection? Sort { get; set; }
        public string? Format { get; }

        public Column(string name, Func<T, object?> getter, string? format = null)
        {
            Name = name;
            ValueGetter = getter;
            Format = format;
        }
    }
}

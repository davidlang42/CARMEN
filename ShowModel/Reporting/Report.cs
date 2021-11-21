using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class Report<T>
    {
        public Column<T>[] AllColumns { get; }
        public IEnumerable<Column<T>> VisibleColumns => AllColumns.Where(c => c.Show).InOrder();
        public T[] Data { get; set; } = Array.Empty<T>();

        public Report(Column<T>[] columns)
        {
            AllColumns = columns;
        }

        public object?[][] GenerateRows()
            => Data.Select(d => VisibleColumns.Select(c => c.ValueGetter(d)).ToArray()).ToArray();

        protected static Column<T>[] AssignOrder(IEnumerable<Column<T>> columns)
        {
            var output = columns.ToArray();
            for (var i = 0; i < output.Length; i++)
                output[i].Order = i;
            return output;
        }
    }
}

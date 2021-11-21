using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class Report<T>
    {
        public Column<T>[] Columns { get; }
        public T[] Data { get; set; } = Array.Empty<T>();
        public List<SortColumn> SortColumns { get; } = new();

        public Report(Column<T>[] columns)
        {
            Columns = columns;
        }

        public object?[][] GenerateRows()
            => Data.Select(d => Columns.Select(c => c.ValueGetter(d)).ToArray()).ToArray();


        protected static Column<T>[] AssignOrder(IEnumerable<Column<T>> columns)
        {
            var output = columns.ToArray();
            for (var i = 0; i < output.Length; i++)
                output[i].Order = i;
            return output;
        }
    }

    public struct SortColumn
    {
        public int ColumnIndex { get; init; }
        public ListSortDirection SortDirection { get; init; }

        public SortColumn(int column_index, ListSortDirection sort_direction)
        {
            ColumnIndex = column_index;
            SortDirection = sort_direction;
        }
    }
}

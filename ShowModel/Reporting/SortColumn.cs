using System.ComponentModel;

namespace Carmen.ShowModel.Reporting
{
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

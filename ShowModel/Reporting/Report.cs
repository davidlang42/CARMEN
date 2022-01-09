using Carmen.ShowModel.Applicants;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public abstract class Report<T> : IReport
    {
        const int MAX_DESCRIPTIONS = 3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract string ReportType { get; }

        protected Column<T>[] columns;
        public IColumn[] Columns => columns;

        public ObservableCollection<SortColumn> SortColumns { get; } = new();

        protected Column<T>? groupColumn;
        public IColumn? GroupColumn
        {
            get => groupColumn;
            set
            {
                if (groupColumn == value)
                    return;
                if (value == null)
                {
                    groupColumn = null;
                    OnPropertyChanged();
                    return;
                }
                foreach (var column in columns)
                    if (column == value)
                    {
                        groupColumn = column;
                        OnPropertyChanged();
                        return;
                    }
                throw new ApplicationException("Tried to set GroupColumn to a column not in this report.");
            }
        }

        private object?[][] rows = Array.Empty<object?[]>();
        public object?[][] Rows
        {
            get => rows;
            set
            {
                if (rows == value)
                    return;
                rows = value;
                OnPropertyChanged();
            }
        }

        private bool exportDescriptiveHeader = false;
        public bool ExportDescriptiveHeader
        {
            get => exportDescriptiveHeader;
            set
            {
                if (exportDescriptiveHeader == value)
                    return;
                exportDescriptiveHeader = value;
                OnPropertyChanged();
            }
        }

        public string ColumnsDescription
        {
            get
            {
                var visible = columns.Where(c => c.Show).InOrder().Select(c => c.Name).ToArray();
                if (visible.Length == 0)
                    return "No columns";
                else if (visible.Length <= MAX_DESCRIPTIONS)
                    return string.Join(", ", visible);
                else
                    return visible.Length.Plural("column");
            }
        }

        public string? SortDescription
        {
            get
            {
                if (SortColumns.Count == 0)
                    return null;
                else if (SortColumns.Count > MAX_DESCRIPTIONS)
                    return string.Join(", ", SortColumns.Select(c => columns[c.ColumnIndex].Name));
                else
                    return string.Join(", ", SortColumns.Select(c => $"{columns[c.ColumnIndex].Name} {c.SortDirection.ToString().ToLower()}"));
            }
        }

        public string FullDescription
        {
            get
            {
                var description = $"{ReportType} with {ColumnsDescription}";
                if (GroupColumn != null)
                    description += $" grouped by {GroupColumn.Name}";
                if (SortColumns.Any())
                    description += $" sorted by {SortDescription}";
                return description;
            }
        }

        public Report(Column<T>[] columns)
        {
            this.columns = columns;
            foreach (var column in columns)
                column.PropertyChanged += Column_PropertyChanged;
            SortColumns.CollectionChanged += SortColumns_CollectionChanged;
        }

        /// <summary>Finds the index of the given column in the current report.
        /// Throws an exception if not found, rather than returning an invalid index.</summary>
        public int IndexOf(IColumn column)
        {
            for (var c = 0; c < columns.Length; c++)
                if (columns[c] == column)
                    return c;
            throw new InvalidOperationException("Column not found.");
        }

        public virtual void SetData(IEnumerable<T> data)
        {   
            Rows = data.Select(d => columns.Select(c => c.ValueGetter(d)).ToArray()).ToArray();
        }

        public int ExportCsv(string file_name)
        {
            var ordered_columns = columns.Where(c => c.Show).InOrder().ToArray();
            if (ordered_columns.Length == 0)
                return 0; // nothing to export
            int count = 0;
            using (var writer = UserException.Handle(() => new StreamWriter(file_name), "Error writing to CSV file."))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (ExportDescriptiveHeader)
                    csv.WriteField(FullDescription);
                else
                    foreach (var column in ordered_columns)
                        csv.WriteField(column.Name);
                csv.NextRecord();
                if (Rows.Length == 0)
                    return 0; // nothing to export after header
                var rows = Sort(Rows, SortColumns);
                if (GroupColumn != null)
                    rows = Group(rows, Rows[0].Length, IndexOf(GroupColumn), IndexOf(ordered_columns.First()));
                foreach (var row in rows)
                {
                    foreach (var column in ordered_columns)
                        csv.WriteField(FormatAsString(row[IndexOf(column)]));
                    csv.NextRecord();
                    count++;
                }
            }
            return count;
        }

        protected string FormatAsString(object? obj)
            => obj switch
            {
                null => "",
                string s => s,
                DateTime dt => $"{dt:dd/MM/yyyy}",
                CastGroup cg => cg.Name,
                AlternativeCast ac => ac.Initial.ToString(),
                bool b => b ? "Y" : "N",
                _ => obj.ToString() ?? ""
            };

        private static IEnumerable<object?[]> Sort(object?[][] rows, IEnumerable<SortColumn> sort_columns)
        {
            IEnumerable<object?[]> sorted_rows = rows;
            foreach (var sort_column in sort_columns.Reverse())
                sorted_rows = sort_column.SortDirection switch
                {
                    ListSortDirection.Ascending => sorted_rows.OrderBy(r => r[sort_column.ColumnIndex]),
                    ListSortDirection.Descending => sorted_rows.OrderByDescending(r => r[sort_column.ColumnIndex]),
                    _ => throw new NotImplementedException($"Enum not handled: {sort_column.SortDirection}")
                };
            return sorted_rows;
        }

        private static IEnumerable<object?[]> Group(IEnumerable<object?[]> rows, int row_length, int group_column_index, int first_column_index)
        {
            object? previous_group = null;
            foreach (var row in rows.OrderBy(r => r[group_column_index]))
            {
                if (!Equals(row[group_column_index], previous_group))
                {
                    var group_header_row = new object?[row_length];
                    previous_group = group_header_row[first_column_index] = row[group_column_index];
                    yield return group_header_row;
                }
                yield return row;
            }
        }

        private void SortColumns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(SortDescription));

        private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(columns));
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Column<T>.Show) || e.PropertyName == nameof(Column<T>.Order))
                OnPropertyChanged(nameof(ColumnsDescription));
        }

        protected static Column<T>[] AssignOrder(IEnumerable<Column<T>> columns)
        {
            var output = columns.ToArray();
            for (var i = 0; i < output.Length; i++)
                output[i].Order = i;
            return output;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

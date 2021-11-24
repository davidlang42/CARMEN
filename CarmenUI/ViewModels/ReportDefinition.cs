using Carmen.ShowModel.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ReportDefinition
    {
        public struct ColumnDefinition
        {
            public string Name;
            public bool Show;
            public int Order;
        }

        public string SavedName { get; set; }
        public string ReportType { get; set; }
        public ColumnDefinition[] Columns { get; set; }
        public SortColumn[] SortColumns { get; set; }
        public int? GroupColumnIndex { get; set; }

        /// <summary>Parameterless constructor required for serialization</summary>
        public ReportDefinition()
        {
            SavedName = "";
            ReportType = "";
            Columns = Array.Empty<ColumnDefinition>();
            SortColumns = Array.Empty<SortColumn>();
        }

        public static ReportDefinition FromReport<T>(Report<T> report, string saved_name)
            => new ReportDefinition
            {
                SavedName = saved_name,
                ReportType = report.ReportType,
                Columns = report.Columns.Select(c => new ColumnDefinition
                {
                    Name = c.Name,
                    Show = c.Show,
                    Order = c.Order
                }).ToArray(),
                SortColumns = report.SortColumns.ToArray(),
                GroupColumnIndex = report.GroupColumn == null ? null : report.IndexOf(report.GroupColumn)
            };

        public void Apply<T>(Report<T> report)
        {
            if (!ReportType.Equals(report.ReportType, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException($"Report definition for '{ReportType}' is not valid for '{report.ReportType}' report.");
            var columns = new Column<T>?[Columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                var definition = Columns[i];
                if (report.Columns.Where(c => c.Name.Equals(definition.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() is Column<T> column)
                {
                    column.Order = definition.Order;
                    column.Show = definition.Show;
                    columns[i] = column;
                }
            }
            report.SortColumns.Clear();
            foreach (var sort_column in SortColumns)
                if (columns[sort_column.ColumnIndex] is Column<T> column)
                    report.SortColumns.Add(new SortColumn
                    {
                        ColumnIndex = report.IndexOf(column),
                        SortDirection = sort_column.SortDirection
                    });
            report.GroupColumn = GroupColumnIndex.HasValue ? columns[GroupColumnIndex.Value] : null;
        }
    }
}

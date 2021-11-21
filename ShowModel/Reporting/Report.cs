﻿using Carmen.ShowModel.Applicants;
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
    public class Report<T> : INotifyPropertyChanged
    {
        const int MAX_DESCRIPTIONS = 3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Column<T>[] Columns { get; }
        public ObservableCollection<SortColumn> SortColumns { get; } = new();

        private Column<T>? groupColumn;
        public Column<T>? GroupColumn
        {
            get => groupColumn;
            set
            {
                if (groupColumn == value)
                    return;
                groupColumn = value;
                OnPropertyChanged();
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

        public string ColumnsDescription
        {
            get
            {
                var visible = Columns.Where(c => c.Show).InOrder().Select(c => c.Name).ToArray();
                if (visible.Length == 0)
                    return "No columns";
                else if (visible.Length <= MAX_DESCRIPTIONS)
                    return string.Join(", ", visible);
                else
                    return visible.Length.Plural("column");
            }
        }

        public string SortDescription
        {
            get
            {
                if (SortColumns.Count > MAX_DESCRIPTIONS)
                    return string.Join(", ", SortColumns.Select(c => Columns[c.ColumnIndex].Name));
                else
                    return string.Join(", ", SortColumns.Select(c => $"{Columns[c.ColumnIndex].Name} {c.SortDirection.ToString().ToLower()}"));
            }
        }

        public Report(Column<T>[] columns)
        {
            Columns = columns;
            foreach (var column in columns)
                column.PropertyChanged += Column_PropertyChanged;
            SortColumns.CollectionChanged += SortColumns_CollectionChanged;
        }

        /// <summary>Finds the index of the given column in the current report.
        /// Throws an exception if not found, rather than returning an invalid index.</summary>
        public int IndexOf(Column<T> column)
        {
            for (var c = 0; c < Columns.Length; c++)
                if (Columns[c] == column)
                    return c;
            throw new InvalidOperationException("Column not found.");
        }

        public void SetData(IEnumerable<T> data)
        {
            Rows = data.Select(d => Columns.Select(c => c.ValueGetter(d)).ToArray()).ToArray();
        }

        public int Export(string file_name)
        {
            int count = 0;
            using (var writer = new StreamWriter(file_name)) //TODO handle file io exceptions
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in Columns)
                    csv.WriteField(column.Name);
                csv.NextRecord();
                foreach (var row in Rows)
                {
                    foreach (var field in row)
                        csv.WriteField(FormatAsString(field));
                    csv.NextRecord();
                    count++;
                }
            }
            return count;
        }

        private string FormatAsString(object? obj)
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

        private void SortColumns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(SortDescription));

        private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
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

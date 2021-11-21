using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Export;
using Carmen.ShowModel.Reporting;
using CarmenUI.Converters;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        ShowConnection connection;

        private ApplicantReport? report;
        public ApplicantReport Report => report ?? throw new ApplicationException("Attempted to access report before initializing.");

        public ReportWindow(ShowConnection connection)
        {
            this.connection = connection;
            InitializeComponent();
        }

        private async void ReportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeReport();
            await RefreshData();
        }

        private async Task InitializeReport()
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(InitializeReport));
            using (var context = ShowContext.Open(connection))
            {
                Criteria[] criterias;
                Tag[] tags;
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                    criterias = await context.Criterias.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Tags), "Tags"))
                    tags = await context.Tags.ToArrayAsync();
                report = new(criterias, tags);
                AddGridColumns(MainData, Report.Columns);
                MainData.DataContext = report;
                columnsCombo.Items.IsLiveSorting = true;
                columnsCombo.Items.SortDescriptions.Add(StandardSort.For<Column<Applicant>>());
                columnsCombo.SelectedIndex = 0;
            }
        }

        private static void AddGridColumns(DataGrid data_grid, Column<Applicant>[] columns)
        {
            data_grid.Columns.Clear();
            for (var c = 0; c < columns.Length; c++)
            {
                var binding = new Binding($"[{c}]"); // this gets parsed in MainData_Sorting
                if (columns[c].Format != null)
                    binding.StringFormat = columns[c].Format;
                var grid_column = new DataGridTextColumn
                {
                    Header = columns[c].Name,
                    Binding = binding,
                    IsReadOnly = true
                };
                data_grid.Columns.Add(grid_column);
                BindingOperations.SetBinding(grid_column, DataGridColumn.VisibilityProperty, new Binding
                {
                    Source = columns[c],
                    Path = new(nameof(Column<Applicant>.Show)),
                    Converter = new BooleanToVisibilityConverter()
                });
            }
            // display index must be set after all columns have been added
            for (var c = 0; c < columns.Length; c++)
                data_grid.Columns[c].DisplayIndex = columns[c].Order;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshData();
        }

        public async Task RefreshData()
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshData));
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
            using (var context = ShowContext.Open(connection))
                Report.SetData(await context.Applicants.ToArrayAsync());
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
            using (loading.Segment(nameof(ConfigureGrouping), "Grouping"))
                ConfigureGrouping(); // TODO ??? must be called every time ItemsSource changes
        }

        private void ConfigureSorting()
        {
            foreach (var sort_column in Report.SortColumns)
            {
                var grid_column = MainData.Columns[sort_column.ColumnIndex];
                grid_column.SortDirection = sort_column.SortDirection;
                MainData.Items.SortDescriptions.Add(new(grid_column.SortMemberPath, sort_column.SortDirection));
            }
        }

        private void ConfigureGrouping() //TODO call this on group combo change, might need overlay
        {
            MainData.Items.GroupDescriptions.Clear();
            if (Report.GroupColumn != null)
                MainData.Items.GroupDescriptions.Add(new PropertyGroupDescription($"[{Report.IndexOf(Report.GroupColumn)}]"));
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var file = new SaveFileDialog
            {
                Title = "Export Applicants to CSV ",
                Filter = "Comma separated values (*.csv)|*.csv|All Files (*.*)|*.*"
            };
            if (file.ShowDialog() == true)
            {
                var count = await ExportApplicants(file.FileName);
                MessageBox.Show($"Exported {count.Plural("applicant")} to {file.FileName}", Title);
            }
        }

        private async Task<int> ExportApplicants(string filename)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(ExportApplicants), "Exporting...");
            using var context = ShowContext.Open(connection);
            Criteria[] criterias;
            Tag[] tags;
            Applicant[] applicants;
            using (loading.Segment(nameof(ShowContext.Criterias), "Criterias"))
                criterias = await context.Criterias.ToArrayAsync();
            using (loading.Segment(nameof(ShowContext.Tags), "Tags"))
                tags = await context.Tags.ToArrayAsync();
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
                applicants = await context.Applicants.ToArrayAsync();
            using (loading.Segment(nameof(CsvExporter), "Writing data"))
            {
                var export = new CsvExporter(criterias, tags);
                return export.Export(filename, applicants);
            }
        }

        private void MainData_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            var index = MainData.Columns.IndexOf(e.Column);
            if (index < 0 || index >= Report.Columns.Length)
                return; // column not found
            Report.Columns[index].Order = e.Column.DisplayIndex;
        }

        private void MainData_Sorting(object sender, DataGridSortingEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                // after sorting has occured
                Report.SortColumns.Clear();
                foreach (var sd in MainData.Items.SortDescriptions)
                {
                    var index = int.Parse(sd.PropertyName[1..^1]); // eg. [0]
                    Report.SortColumns.Add(new(index, sd.Direction));
                }
            });
        }
    }
}

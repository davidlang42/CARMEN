using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Export;
using Carmen.ShowModel.Reporting;
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
        Column<Applicant>[] loadedColumns;

        private ApplicantReport? report;
        public ApplicantReport Report => report ?? throw new ApplicationException("Attempted to access report before initializing.");

        public ReportWindow(ShowConnection connection)
        {
            loadedColumns = Array.Empty<Column<Applicant>>();
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
            }
        }

        public async Task RefreshData()
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshData));
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
            using (var context = ShowContext.Open(connection))
                Report.Data = await context.Applicants.ToArrayAsync();
            using (loading.Segment(nameof(PopulateGrid), "Generating report"))
                PopulateGrid();
        }

        private void PopulateGrid()
        {
            MainData.Columns.Clear();
            int array_index = 0;
            loadedColumns = Report.VisibleColumns.ToArray();
            foreach (var column in loadedColumns)
            {
                var binding = new Binding($"[{array_index++}]");
                if (column.Format != null)
                    binding.StringFormat = column.Format;
                MainData.Columns.Add(new DataGridTextColumn
                {
                    Header = column.Name,
                    Binding = binding,
                    IsReadOnly = true
                });
            }
            MainData.ItemsSource = Report.GenerateRows();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshData();
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
            if (index < 0 || index >= loadedColumns.Length)
                return; // column not found
            var column = loadedColumns[index];
            column.Order = e.Column.DisplayIndex;
        }
    }
}

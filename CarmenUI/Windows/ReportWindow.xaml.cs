using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Reporting;
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
            foreach (var column in Report.VisibleColumns)
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
    }
}

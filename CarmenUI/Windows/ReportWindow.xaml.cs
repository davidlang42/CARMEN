using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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

        public ApplicantReport Report { get; }

        public ReportWindow(ShowConnection connection)
        {
            this.connection = connection;
            InitializeComponent();
            Report = new();
        }

        private async void ReportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshData();
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

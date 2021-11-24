using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Reporting;
using CarmenUI.Converters;
using CarmenUI.ViewModels;
using FontAwesome.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        ShowConnection connection;
        ReportDefinition? reportDefinition;
        string defaultTitle;

        private ApplicantsReport? report;
        public ApplicantsReport Report => report ?? throw new ApplicationException("Attempted to access report before initializing.");

        public ReportWindow(ShowConnection connection, string default_title, ReportDefinition? report_definition = null)
        {
            defaultTitle = default_title;
            this.connection = connection;
            this.reportDefinition = report_definition;
            InitializeComponent(); //TODO fix binding errors
            UpdateBookmarkIconAndReportTitle();
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
                using (loading.Segment(nameof(AddGridColumns), "Report"))
                {
                    if (reportDefinition != null && ReportTypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => reportDefinition.ReportType.Equals(cbi.Content)) is ComboBoxItem item)
                        item.IsSelected = true;
                    report = (ReportTypeCombo.SelectedItem as ComboBoxItem)?.Content switch
                    {
                        "All Applicants" => new ApplicantsReport(criterias, tags),
                        "Accepted Applicants" => new AcceptedApplicantsReport(criterias, tags),
                        "Rejected Applicants" => new RejectedApplicantsReport(criterias, tags),
                        _ => throw new ApplicationException($"Report type combo not handled: {ReportTypeCombo.SelectedItem}")
                    };
                    if (reportDefinition != null)
                        reportDefinition.Apply(report);
                    report.PropertyChanged += Report_PropertyChanged;
                    AddGridColumns(MainData, Report.Columns);
                    MainData.DataContext = report;
                    columnsCombo.Items.IsLiveSorting = true;
                    columnsCombo.Items.SortDescriptions.Add(StandardSort.For<Column<Applicant>>());
                    columnsCombo.SelectedIndex = 0;
                    ConfigureGrouping();
                }   
            }
        }

        private void Report_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Report<Applicant>.Rows))
            {
                reportDefinition = null;
                UpdateBookmarkIconAndReportTitle();
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
            {
                var data = await context.Applicants.ToArrayAsync();
                Report.SetData(data);
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        private void ConfigureSorting()
        {
            MainData.Items.SortDescriptions.Clear();
            if (Report.GroupColumn != null)
                MainData.Items.SortDescriptions.Add(new($"[{Report.IndexOf(Report.GroupColumn)}]", ListSortDirection.Ascending));
            foreach (var sort_column in Report.SortColumns)
            {
                var grid_column = MainData.Columns[sort_column.ColumnIndex];
                grid_column.SortDirection = sort_column.SortDirection;
                MainData.Items.SortDescriptions.Add(new(grid_column.SortMemberPath, sort_column.SortDirection));
            }
        }

        private void GroupColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(GroupColumn_SelectionChanged));
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting();
            using (loading.Segment(nameof(ConfigureGrouping), "Grouping"))
                ConfigureGrouping();
        }    

        private void ConfigureGrouping()
        {
            MainData.Items.GroupDescriptions.Clear();
            if (Report.GroupColumn != null)
                MainData.Items.GroupDescriptions.Add(new PropertyGroupDescription($"[{Report.IndexOf(Report.GroupColumn)}]"));
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var default_file_name = (reportDefinition?.SavedName ?? Report.FullDescription).Replace(".","") + ".csv";
            default_file_name = string.Concat(default_file_name.Split(Path.GetInvalidFileNameChars()));
            var file = new SaveFileDialog
            {
                Title = "Export Applicants to CSV ",
                Filter = "Comma separated values (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = default_file_name
            };
            if (file.ShowDialog() == true)
            {
                int count;
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(ExportButton_Click), "Exporting..."))
                    count = Report.Export(file.FileName);
                MessageBox.Show($"Exported {count.Plural("row")} to {file.FileName}", Title);
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
                IEnumerable<SortDescription> sort_descriptions = MainData.Items.SortDescriptions;
                if (Report.GroupColumn != null)
                    sort_descriptions = sort_descriptions.Skip(1);
                Report.SortColumns.Clear();
                foreach (var sd in sort_descriptions)
                {
                    var index = int.Parse(sd.PropertyName[1..^1]); // eg. [0]
                    Report.SortColumns.Add(new(index, sd.Direction));
                }
            });
        }

        private void ClearGrouping_Click(object sender, RoutedEventArgs e)
        {
            Report.GroupColumn = null;
        }

        private void ClearSorting_Click(object sender, RoutedEventArgs e)
        {
            Report.SortColumns.Clear(); //TODO fix, doesnt actually remove sorting
        }

        private async void ReportTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (report != null) // might still be loading
            {
                reportDefinition = null;
                UpdateBookmarkIconAndReportTitle();
                if (!report.ReportType.Equals((ReportTypeCombo.SelectedItem as ComboBoxItem)?.Content))
                    await InitializeReport();
                await RefreshData();
            }
        }

        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (reportDefinition == null)
                AddBookmark();
            else
                RemoveBookmark();
            UpdateBookmarkIconAndReportTitle();
        }

        private void UpdateBookmarkIconAndReportTitle()
        {
            if (reportDefinition == null)
            {
                BookmarkIcon.Icon = FontAwesomeIcon.BookmarkOutline;
                Title = defaultTitle;
            }
            else
            {
                BookmarkIcon.Icon = FontAwesomeIcon.Bookmark;
                Title = "Report: " + reportDefinition.SavedName;
            }
        }

        private void AddBookmark()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("What should this saved report be called?", Title, Report.FullDescription); // I'm sorry
            if (!string.IsNullOrWhiteSpace(name))
            {
                reportDefinition = ReportDefinition.FromReport(Report, name);
                Properties.Settings.Default.ReportDefinitions.Add(reportDefinition);
            }
        }

        private void RemoveBookmark()
        {
            if (MessageBox.Show($"Are you sure you want to remove the '{reportDefinition?.SavedName}' saved report?", Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.ReportDefinitions.Remove(reportDefinition);
                reportDefinition = null;
            }
        }

        private async void WindowRoot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Properties.Settings.Default.RefreshOnF5 && e.Key == Key.F5 && Keyboard.Modifiers == ModifierKeys.None)
            {
                await RefreshData();
                e.Handled = true;
            }
            else if (Properties.Settings.Default.ReportOnCtrlR && e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ((MainWindow)Owner).OpenReport(null);
                e.Handled = true;
            }
        }
    }
}

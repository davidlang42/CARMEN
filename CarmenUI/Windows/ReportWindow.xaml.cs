﻿using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Reporting;
using Carmen.ShowModel.Structure;
using CarmenUI.Converters;
using CarmenUI.ViewModels;
using FontAwesome.WPF;
using Microsoft.Win32;
using Serilog;
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

        private IReport? report;
        public IReport Report => report ?? throw new ApplicationException("Attempted to access report before initializing.");

        public ReportWindow(ShowConnection connection, string default_title, ReportDefinition? report_definition = null)
        {
            Log.Information(nameof(ReportWindow));
            defaultTitle = default_title;
            this.connection = connection;
            this.reportDefinition = report_definition;
            InitializeComponent();
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
                //TODO pre-load conditionally based on which report
                Criteria[] criterias;
                Tag[] tags;
                CastGroup[] cast_groups;
                AlternativeCast[] alternative_casts;
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                    criterias = await context.Criterias.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Tags), "Tags"))
                    tags = await context.Tags.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.CastGroups), "Cast Groups"))
                    cast_groups = await context.CastGroups.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative Casts"))
                    alternative_casts = await context.AlternativeCasts.ToArrayAsync();
                using (loading.Segment(nameof(AddGridColumns), "Report"))
                {
                    if (reportDefinition != null && ReportTypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => reportDefinition.ReportType.Equals(cbi.Content)) is ComboBoxItem item)
                        item.IsSelected = true;
                    report = (ReportTypeCombo.SelectedItem as ComboBoxItem)?.Content switch
                    {
                        "All Applicants" => new ApplicantsReport(criterias, tags),
                        "Accepted Applicants" => new AcceptedApplicantsReport(criterias, tags),
                        "Rejected Applicants" => new RejectedApplicantsReport(criterias, tags),
                        "Items" => new ItemsReport(cast_groups),
                        "Roles" => new RolesReport(cast_groups, alternative_casts),
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

        private static void AddGridColumns(DataGrid data_grid, IColumn[] columns)
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
            if (Report is Report<Applicant> applicant_report)
                await RefreshApplicantData(applicant_report);
            else if (Report is Report<Item> item_report)
                await RefreshItemData(item_report);
            else if (Report is Report<(Item, Role)> roles_report)
                await RefreshRolesData(roles_report);
            else
                throw new ApplicationException("Report type not handled: " + Report.GetType().Name);
        }

        public async Task RefreshApplicantData(Report<Applicant> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshApplicantData));
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
            using (var context = ShowContext.Open(connection))
            {
                var applicants = await context.Applicants.ToArrayAsync();
                report.SetData(applicants);
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        public async Task RefreshItemData(Report<Item> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshItemData));
            using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item), "Items"))//TODO this could be broken down better I think
            using (var context = ShowContext.Open(connection))
            {
                await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                var items_in_order = context.ShowRoot.ItemsInOrder();
                report.SetData(items_in_order);
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        public async Task RefreshRolesData(Report<(Item, Role)> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshRolesData));
            using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item), "Roles"))//TODO this could be broken down better I think
            using (var context = ShowContext.Open(connection))
            {
                await context.Roles.Include(r => r.Items).LoadAsync();
                await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                var items_in_order = context.ShowRoot.ItemsInOrder();
                report.SetData(EnumerateRoles(items_in_order));
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        private static IEnumerable<(Item, Role)> EnumerateRoles(IEnumerable<Item> items)
        {
            foreach (var item in items)
                foreach (var role in item.Roles)
                    yield return (item, role);
        }

        private void ConfigureSorting()
        {
            MainData.Items.SortDescriptions.Clear();
            if (Report.GroupColumn != null)
                MainData.Items.SortDescriptions.Add(new($"[{Report.IndexOf(Report.GroupColumn)}]", ListSortDirection.Ascending));
            foreach (var column in MainData.Columns)
                column.SortDirection = null;
            foreach (var sort_column in Report.SortColumns)
            {
                var grid_column = MainData.Columns[sort_column.ColumnIndex];
                grid_column.SortDirection = sort_column.SortDirection;
                MainData.Items.SortDescriptions.Add(new(grid_column.SortMemberPath, sort_column.SortDirection));
            }
        }

        private void GroupColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingOverlay.CurrentOverlay != null)
                return; // already loading something, which will call ConfigureSort/Group themselves
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

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var default_file_name = (reportDefinition?.SavedName ?? Report.FullDescription).Replace(".","") + ".csv";
            default_file_name = string.Concat(default_file_name.Split(Path.GetInvalidFileNameChars()));
            var file = new SaveFileDialog
            {
                Title = "Export Applicants to CSV",
                Filter = "Comma separated values (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = default_file_name
            };
            if (file.ShowDialog() == true)
            {
                int count;
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(ExportCsv_Click), "Exporting..."))
                    count = Report.ExportCsv(file.FileName);
                MessageBox.Show($"Exported {count.Plural("row")} to {file.FileName}", Title);
            }
        }

        private async void ExportPhotos_Click(object sender, RoutedEventArgs e) //TODO hide export photos option when non-applicant report
        {
            if (Report is not ApplicantsReport applicant_report)
            {
                MessageBox.Show("Only applicants reports can export as photos.", Title);
                return;
            }
            var default_file_name = (reportDefinition?.SavedName ?? Report.FullDescription).Replace(".", "") + ".zip";
            default_file_name = string.Concat(default_file_name.Split(Path.GetInvalidFileNameChars()));
            var file = new SaveFileDialog
            {
                Title = "Export Photos as ZIP",
                Filter = "Zip archive (*.zip)|*.zip|All Files (*.*)|*.*",
                FileName = default_file_name
            };
            if (file.ShowDialog() == true)
            {
                if (File.Exists(file.FileName))
                    UserException.Handle(() => File.Delete(file.FileName), "Error overwriting file."); // user has accepted overwrite in the SaveFileDialog
                int count;
                using (var loading = new LoadingOverlay(this) { MainText = "Exporting..." })
                using (var context = ShowContext.Open(connection))
                {
                    loading.SubText = "Loading applicants";
                    var applicants = await context.Applicants.ToArrayAsync();
                    count = await applicant_report.ExportPhotos(file.FileName, applicants, i =>
                    {
                        loading.Progress = 100 * i / applicants.Length;
                        loading.SubText = $"Applicant {i + 1}/{applicants.Length}";
                    });
                }
                MessageBox.Show($"Exported {count.Plural("photos")} to {file.FileName}", Title);
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
            Report.SortColumns.Clear();
            ConfigureSorting();
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

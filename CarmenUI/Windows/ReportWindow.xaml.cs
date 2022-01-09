using Carmen.ShowModel;
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
        bool clearDefinitionAfterLoading;
        string defaultTitle;

        private IReport? report;
        public IReport Report => report ?? throw new ApplicationException("Attempted to access report before initializing.");

        public ReportDefinition? ReportDefinition { get; private set; }

        public ReportWindow(ShowConnection connection, string default_title)
            : this(connection, default_title, null, false)
        { }

        public ReportWindow(ShowConnection connection, string default_title, ReportDefinition? report_definition, bool clear_definition_after_loading)
        {
            Log.Information(nameof(ReportWindow));
            defaultTitle = default_title;
            this.connection = connection;
            ReportDefinition = report_definition;
            clearDefinitionAfterLoading = clear_definition_after_loading;
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
                Criteria[] criterias;
                Tag[] tags;
                CastGroup[] cast_groups;
                AlternativeCast[] alternative_casts;
                List<int> item_ids_in_order;
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                    criterias = await context.Criterias.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Tags), "Tags"))
                    tags = await context.Tags.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.CastGroups), "Cast groups"))
                    cast_groups = await context.CastGroups.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                    alternative_casts = await context.AlternativeCasts.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(InnerNode) + nameof(InnerNode.Children), "Nodes"))
                {
                    await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                    item_ids_in_order = context.ShowRoot.ItemsInOrder().Select(i => i.NodeId).ToList();
                }
                using (loading.Segment(nameof(AddGridColumns), "Report"))
                {
                    if (ReportDefinition != null && ReportTypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault(cbi => ReportDefinition.ReportType.Equals(cbi.Content)) is ComboBoxItem item)
                        item.IsSelected = true;
                    if (report != null)
                        report.PropertyChanged -= Report_PropertyChanged;
                    report = (ReportTypeCombo.SelectedItem as ComboBoxItem)?.Content switch
                    {
                        ApplicantsReport.DefaultReportType => new ApplicantsReport(criterias, tags),
                        AcceptedApplicantsReport.DefaultReportType => new AcceptedApplicantsReport(criterias, tags),
                        RejectedApplicantsReport.DefaultReportType => new RejectedApplicantsReport(criterias, tags),
                        ItemsReport.DefaultReportType => new ItemsReport(item_ids_in_order, cast_groups),
                        RolesReport.DefaultReportType => new RolesReport(item_ids_in_order, cast_groups, alternative_casts),
                        CastingReport.DefaultReportType => new CastingReport(item_ids_in_order, cast_groups, alternative_casts, criterias, tags),
                        _ => throw new ApplicationException($"Report type combo not handled: {ReportTypeCombo.SelectedItem}")
                    };
                    if (ReportDefinition != null)
                    {
                        ReportDefinition.Apply(report);
                        if (clearDefinitionAfterLoading)
                            ReportDefinition = null;
                    }
                    AddGridColumns(MainData, Report.Columns);
                    MainData.DataContext = report;
                    columnsCombo.Items.IsLiveSorting = true;
                    columnsCombo.Items.SortDescriptions.Add(StandardSort.For<Column<Applicant>>());
                    columnsCombo.SelectedIndex = 0;
                    ConfigureGrouping();
                    report.PropertyChanged += Report_PropertyChanged;
                }   
            }
        }

        private void Report_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IReport.Rows))
            {
                ReportDefinition = null;
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
            else if (Report is Report<(Item, Role, Applicant)> casting_report)
                await RefreshCastingData(casting_report);
            else
                throw new ApplicationException("Report type not handled: " + Report.GetType().Name);
        }

        public async Task RefreshApplicantData(Report<Applicant> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshApplicantData));
            using (var context = ShowContext.Open(connection))
            {
                Applicant[] applicants;
                using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.Notes), "Applicants"))
                    applicants = await context.Applicants.Include(a => a.Notes).ToArrayAsync();
                using (loading.Segment(nameof(Report<Applicant>) + nameof(Applicant) + nameof(Report<Applicant>.SetData), "Applicants"))
                    await report.SetData(applicants);
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        public async Task RefreshItemData(Report<Item> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshItemData));
            using (var context = ShowContext.Open(connection))
            {
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(InnerNode) + nameof(InnerNode.Children), "Nodes"))
                    await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                using (loading.Segment(nameof(Report<Item>) + nameof(Item) + nameof(Report<Item>.SetData), "Items"))
                    await report.SetData(context.ShowRoot.ItemsInOrder());
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        public async Task RefreshRolesData(Report<(Item, Role)> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshRolesData));
            using (var context = ShowContext.Open(connection))
            {
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(InnerNode) + nameof(InnerNode.Children), "Nodes"))
                    await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Roles) + nameof(Role.Items), "Roles"))
                    await context.Roles.Include(r => r.Items).LoadAsync();
                using (loading.Segment(nameof(Report<(Item, Role)>) + nameof(Item) + nameof(Role) + nameof(Report<(Item, Role)>.SetData), "Items"))
                    await report .SetData(EnumerateRoles(context.ShowRoot.ItemsInOrder()));
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

        public async Task RefreshCastingData(Report<(Item, Role, Applicant)> report)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshCastingData));
            using (var context = ShowContext.Open(connection))
            {
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(InnerNode) + nameof(InnerNode.Children), "Nodes"))
                    await context.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Roles) + nameof(Role.Items), "Roles"))
                    await context.Roles.Include(r => r.Items).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.Roles), "Applicants"))
                    await context.Applicants.Where(a => a.CastGroup != null).Include(a => a.Roles).LoadAsync();
                using (loading.Segment(nameof(Report<(Item, Role, Applicant)>) + nameof(Item) + nameof(Role) + nameof(Applicant) + nameof(Report<(Item, Role, Applicant)>.SetData), "Casting"))
                    await report .SetData(EnumerateCasting(context.ShowRoot.ItemsInOrder()));
            }
            using (loading.Segment(nameof(ConfigureSorting), "Sorting"))
                ConfigureSorting(); // must be called every time ItemsSource changes
        }

        private static IEnumerable<(Item, Role, Applicant)> EnumerateCasting(IEnumerable<Item> items)
        {
            foreach (var item in items)
                foreach (var role in item.Roles)
                    foreach (var applicant in role.Cast)
                        yield return (item, role, applicant);
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
            var default_file_name = (ReportDefinition?.SavedName ?? Report.FullDescription).Replace(".","") + ".csv";
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

        private async void ExportPhotos_Click(object sender, RoutedEventArgs e)
        {
            if (Report is not ApplicantsReport applicant_report)
            {
                MessageBox.Show("Only applicants reports can export as photos.", Title);
                return;
            }
            var default_file_name = (ReportDefinition?.SavedName ?? Report.FullDescription).Replace(".", "") + ".zip";
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
                ReportDefinition = null;
                UpdateBookmarkIconAndReportTitle();
                if (!report.ReportType.Equals((ReportTypeCombo.SelectedItem as ComboBoxItem)?.Content))
                    await InitializeReport();
                await RefreshData();
            }
        }

        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReportDefinition == null || clearDefinitionAfterLoading)
                AddBookmark();
            else
                RemoveBookmark();
            UpdateBookmarkIconAndReportTitle();
        }

        private void UpdateBookmarkIconAndReportTitle()
        {
            if (ReportDefinition == null || clearDefinitionAfterLoading)
            {
                BookmarkIcon.Icon = FontAwesomeIcon.BookmarkOutline;
                Title = defaultTitle;
            }
            else
            {
                BookmarkIcon.Icon = FontAwesomeIcon.Bookmark;
                Title = "Report: " + ReportDefinition.SavedName;
            }
        }

        private void AddBookmark()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("What should this saved report be called?", Title, Report.FullDescription); // I'm sorry
            if (!string.IsNullOrWhiteSpace(name))
            {
                ReportDefinition = ReportDefinition.FromReport(Report, name);
                Properties.Settings.Default.ReportDefinitions.Add(ReportDefinition);
            }
        }

        private void RemoveBookmark()
        {
            if (MessageBox.Show($"Are you sure you want to remove the '{ReportDefinition?.SavedName}' saved report?", Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.ReportDefinitions.Remove(ReportDefinition);
                ReportDefinition = null;
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
                ((MainWindow)Owner).OpenReport(ReportDefinition.FromReport(Report, ""), false);
                e.Handled = true;
            }
        }
    }
}

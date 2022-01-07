using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Carmen.ShowModel;
using C = Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using CarmenUI.UserControls;
using System.Threading.Tasks;
using Serilog;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, IDisposable
    {
        readonly RecentShow connection;

        public ShowSummary ShowSummary { get; init; }
        public RegistrationSummary RegistrationSummary { get; init; } = new();
        public AuditionSummary AuditionSummary { get; init; } = new();
        public CastSummary CastSummary { get; init; } = new();
        public ItemsSummary ItemsSummary { get; init; } = new();
        public RolesSummary RolesSummary { get; init; } = new();

        private Summary[] allSummaries => new Summary[] { ShowSummary, RegistrationSummary, AuditionSummary, CastSummary, ItemsSummary, RolesSummary };
        private CancellationTokenSource? updatingSummaries;

        public MainMenu(RecentShow connection)
        {
            this.connection = connection;
            ShowSummary = new(connection.DefaultShowName);
            InitializeComponent();
            DatabaseButton.Visibility = connection.Provider.HasValue ? Visibility.Visible : Visibility.Hidden;
        }

        public void Dispose()
        {
            if (updatingSummaries != null)
            {
                updatingSummaries.Dispose();
                updatingSummaries = null;
            }
        }

        /// <summary>Handles return events from sub-pages (e.Result returns what has changed,
        /// even on cancel, because changes might have been saved at another time)</summary>
        private void HandleChangesOnReturn(object sender, ReturnEventArgs<DataObjects> e)
            => InvalidateSummaries(e.Result);

        private void NavigateToSubPage(SubPage sub_page)
        {
            Log.Information($"Navigate to {sub_page.Title}");
            sub_page.Return += HandleChangesOnReturn;
            this.NavigationService.Navigate(sub_page);
        }

        private void menuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListViewItem selected) {
                if (selected == ConfigureShow)
                    NavigateToSubPage(new ConfigureShow(connection));
                else if (selected == AllocateRoles)
                    NavigateToSubPage(new AllocateRoles(connection));
                else if (selected == ConfigureItems)
                    NavigateToSubPage(new ConfigureItems(connection));
                else if (selected == SelectCast)
                    NavigateToSubPage(new SelectCast(connection));
                else if (selected == AuditionApplicants)
                    NavigateToSubPage(new EditApplicants(connection, EditApplicantsMode.AuditionApplicants));
                else if (selected == RegisterApplicants)
                    NavigateToSubPage(new EditApplicants(connection, EditApplicantsMode.RegisterApplicants));
                menuList.SelectedItem = null;
            }
        }

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = ShowSummary;

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = RegistrationSummary;

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = AuditionSummary;

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = CastSummary;

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = ItemsSummary;

        private void AllocateRoles_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = RolesSummary;

        private void Page_Initialized(object sender, EventArgs e)
            => InvalidateSummaries(DataObjects.All);

        private void InvalidateSummaries(DataObjects changes)
        {
            // Determine which summaries need updating
            HashSet<Summary> summaries = new();
            if (changes.HasFlag(DataObjects.Nodes)
                || changes.HasFlag(DataObjects.Criterias)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.AlternativeCasts)
                || changes.HasFlag(DataObjects.Tags)
                || changes.HasFlag(DataObjects.SectionTypes)
                || changes.HasFlag(DataObjects.Requirements))
                summaries.Add(ShowSummary);
            if (changes.HasFlag(DataObjects.Applicants)
                || changes.HasFlag(DataObjects.Abilities)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.AlternativeCasts)
                || changes.HasFlag(DataObjects.Requirements)
                || changes.HasFlag(DataObjects.Criterias))
                summaries.AddRange(new Summary[] { RegistrationSummary, AuditionSummary });
            if (changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.AlternativeCasts)
                || changes.HasFlag(DataObjects.SameCastSets)
                || changes.HasFlag(DataObjects.Tags)
                || changes.HasFlag(DataObjects.Applicants))
                summaries.Add(CastSummary);
            if (changes.HasFlag(DataObjects.Nodes)
                || changes.HasFlag(DataObjects.Roles)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.AlternativeCasts)
                || changes.HasFlag(DataObjects.SectionTypes)
                || changes.HasFlag(DataObjects.Applicants)
                || changes.HasFlag(DataObjects.AllowedConsecutives))
                summaries.AddRange(new Summary[] { ItemsSummary, RolesSummary });
            // Trigger update if required
            if (summaries.Count == 0)
                return; // nothing to do
            if (updatingSummaries != null)
            {
                updatingSummaries.Cancel();
                updatingSummaries.Dispose();
            }
            updatingSummaries = new();
            UpdateSummariesAsync(summaries, updatingSummaries.Token);
        }

        private async void UpdateSummariesAsync(IEnumerable<Summary> summaries, CancellationToken cancel)
        {
            // Mark them as 'Loading'
            foreach (var summary in summaries)
                summary.Status = ProcessStatus.Loading;
            CastingComplete.Visibility = Visibility.Hidden;
            // Use a short-lived context
            using var context = ShowContext.Open(connection);
            // Update summaries sequentially
            foreach (var summary in allSummaries.Where(s => summaries.Contains(s)))
            {
                Log.Information($"Update summary {summary.GetType().Name}");
                await summary.LoadAsync(context, cancel);
                if (cancel.IsCancellationRequested)
                    return;
            }  
            // Show complete text if required
            if (allSummaries.All(s => s.Status == ProcessStatus.Complete))
            {
                Log.Information($"IT'S SHOWTIME");
                CastingComplete.Visibility = Visibility.Visible;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.ExitToStartOnBackspace && e.Key == Key.Back)
            {
                BackButton_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.OpenSettingsOnF1 && e.Key == Key.F1)
            {
                SettingsButton_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.ShortcutsOnMainMenu)
            {
                var list_item = e.Key switch
                {
                    Key.S => ConfigureShow,
                    Key.P => RegisterApplicants,
                    Key.A => AuditionApplicants,
                    Key.C => SelectCast,
                    Key.I => ConfigureItems,
                    Key.R => AllocateRoles,
                    _ => null
                };
                if (list_item != null)
                {
                    e.Handled = true;
                    menuList_SelectionChanged(sender, new(e.RoutedEvent, Array.Empty<object>(), new[] { list_item }));
                }
            }
        }

        Window? window;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("Loaded MainMenu");
            window = Window.GetWindow(this);
            window.KeyDown += Window_KeyDown;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Log.Information("Unloaded MainMenu");
            if (window != null)
                window.KeyDown -= Window_KeyDown;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Exited MainMenu");
            var start = new StartWindow();
            start.Show();
            this.Close();
        }

        private void OpenReportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                ReportsPopup.IsOpen = false;
            ((MainWindow)Window.GetWindow(this)).OpenReport(e.Parameter as ReportDefinition);
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ReportDefinitions.Count == 0)
            {
                ReportsPopup.IsOpen = false;
                ((MainWindow)Window.GetWindow(this)).OpenReport(null);
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                var msg = $"This will DELETE all existing data in the currently open show, including applicant details, audition marks, and casting information, and replace it with only the data in '{dialog.FileName}'. THIS IS IRREVERSABLE, the existing data will be lost forever.\n\nAre you sure you want to overwrite all existing data?";
                if (MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                msg = $"WARNING!!! You should only do this if you are the person in charge. This will affect ALL users of this database, and there will be no way to recover the data after this point.\n\n{msg}";
                if (MessageBox.Show(msg, "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    MessageBox.Show("Good choice.", WindowTitle);
                    return;
                }
                var backup_file = $"{Path.GetTempPath()}backup_before_import_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                // create backup
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(ExportButton_Click), "Backing up..."))
                using (var current_show = ShowContext.Open(connection))
                {
                    var segment = loading.Segment(nameof(ShowContext.CopyDatabase), "Creating backup");
                    await current_show.CopyDatabase(new LocalShowConnection(backup_file), (key, display) =>
                    {
                        segment.Dispose();
                        segment = loading.Segment(key, display);
                    });
                    segment.Dispose();
                }
                // delete and import
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(ImportButton_Click), "Importing..."))
                using (var import_from = ShowContext.Open(new LocalShowConnection(dialog.FileName)))
                {
                    var segment = loading.Segment(nameof(ShowContext.CopyDatabase), "Clearing database");
                    await import_from.CopyDatabase(connection, (key, display) =>
                    {
                        segment.Dispose();
                        segment = loading.Segment(key, display);
                    });
                    segment.Dispose();
                }
                MessageBox.Show($"Imported database from: {dialog.FileName}\nBackup saved to: {backup_file}");
                InvalidateSummaries(DataObjects.All);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                if (File.Exists(dialog.FileName))
                {
                    if (MessageBox.Show($"This will delete '{dialog.FileName}' and create a new database in its place. Are you sure you want to continue?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    FileSystem.DeleteFile(dialog.FileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(ExportButton_Click), "Exporting..."))
                using (var source = ShowContext.Open(connection))
                {
                    var segment = loading.Segment(nameof(ShowContext.CopyDatabase), "Creating database");
                    await source.CopyDatabase(new LocalShowConnection(dialog.FileName), (key, display) =>
                    {
                        segment.Dispose();
                        segment = loading.Segment(key, display);
                    });
                    segment.Dispose();
                }
                MessageBox.Show($"Exported database to: {dialog.FileName}");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Closing += Settings_Closing;
            settings.Show();
            this.Close();
        }

        private void Settings_Closing(object sender, EventArgs e)
        {
            var settings = (SettingsWindow)sender;
            using var loading = new LoadingOverlay(settings);
            var new_main_window = new MainWindow(connection);
            new_main_window.Show();
        }

        private void Close()
        {
            var this_window = Window.GetWindow(this);
            this_window.Close();
        }

        private async void RefreshImageCache_Click(object sender, RoutedEventArgs e)
        {
            using var context = ShowContext.Open(connection);
            HashSet<int> image_ids;
            Dictionary<int, string> cached_ids = new();
            using (var loading = new LoadingOverlay(this).AsSegment(nameof(RefreshImageCache_Click), "Refreshing..."))
            {
                using (loading.Segment(nameof(ShowContext.Images), "Image list"))
                    image_ids = await context.Images.Select(i => i.ImageId).ToHashSetAsync();
                using (loading.Segment(nameof(ApplicantImage.GetCachePath), "Cache list"))
                {
                    var cache_path = ApplicantImage.GetCachePath(context.ShowRoot);
                    if (Directory.Exists(cache_path))
                    {
                        foreach (var cached_file in UserException.Handle(() => Directory.GetFiles(cache_path), "Error reading image cache path."))
                        {
                            var filename = Path.GetFileName(cached_file);
                            if (MatchImageId(filename, out var image_id) && image_ids.Contains(image_id))
                                cached_ids.Add(image_id, cached_file);
                            else
                                UserException.Handle(() => File.Delete(cached_file), "Error deleting cached images.");
                        }
                    }
                }
            }
            if (cached_ids.Any())
            {
                if (MessageBox.Show($"Do you want to clear the existing {cached_ids.Count} images in the cache?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    foreach (var (_, cached_file) in cached_ids)
                        UserException.Handle(() => File.Delete(cached_file), "Error deleting cached images.");
                else
                    foreach (var (id, _) in cached_ids)
                        image_ids.Remove(id);
            }
            using (var loading = new LoadingOverlay(this) { MainText = "Refreshing..." })
            {
                var i = 0;
                foreach (var image_id in image_ids)
                {
                    loading.Progress = 100 * i / image_ids.Count;
                    loading.SubText = $"Loading image {++i}/{image_ids.Count}";
                    await Task.Run(() => ApplicantImage.CachedImage(image_id, context.ShowRoot, () => context.Images.Single(i => i.ImageId == image_id)));
                }
            }
            MessageBox.Show($"Loaded and cached {image_ids.Count.Plural("image")}.", WindowTitle);
        }

        private bool MatchImageId(string filename, out int image_id)
        {
            image_id = default;
            var ext = "." + ApplicantImage.ImageCacheExtension;
            return filename.EndsWith(ext) && int.TryParse(filename[0..^ext.Length], out image_id);
        }
    }
}

using CarmenUI.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for LoadDatabase.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        const int MAX_RECENT_SHOWS = 4;

        public StartWindow()
        {
            InitializeComponent();
        }

        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                var show = new RecentShow { Filename = dialog.FileName };
#if DEBUG
                bool add_test_data = MessageBox.Show("Do you want to add test data?", "DEBUG", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
#endif
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(StartWindow) + nameof(NewButton_Click)))
                using (var context = ShowContext.Open(show))
                {
                    using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.PreloadModel), "Preparing show model"))
                        await context.PreloadModel(); // do this here while the overlay is shown to avoid a synchronous delay when the MainMenu is loaded
                    using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.CreateNewDatabase), "Creating new database"))
                        await context.CreateNewDatabase(show.DefaultShowName);
#if DEBUG
                    if (add_test_data)
                        using (loading.Segment(nameof(StartWindow) + nameof(AddTestData), "Generating test data"))
                            await Task.Run(async () => await AddTestData(context));
#endif
                }
                using (new LoadingOverlay(this) { SubText = "Opening show" })
                    LaunchMainWindow(show);
            }
        }

#if DEBUG
        private static async Task AddTestData(ShowContext context)
        {
            using var test_data = new TestDataGenerator(context, 0);
            test_data.AddAlternativeCasts();
            test_data.AddCastGroups(4);
            await context.SaveChangesAsync();
            test_data.AddShowStructure(30, 6, 1, include_items_at_every_depth: false); // after cast groups committed
            await context.SaveChangesAsync();
            test_data.AddCriteriaAndRequirements(); // after cast groups committed
            await context.SaveChangesAsync();
            test_data.AddTags(1); // after requirements committed
            context.SaveChanges();
            test_data.AddApplicants(100); // after criteria, tags, alternative casts committed
            await context.SaveChangesAsync();
            test_data.AddRoles(5); // after applicants, items, cast groups, requirements committed
            test_data.AddImages(); // after applicants, tags committed
            await context.SaveChangesAsync();
        }
#endif

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Existing Database",
                Filter = "Sqlite Database (*.db)|*.db|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                await OpenShow(new RecentShow { Filename = dialog.FileName});
        }

        private async Task OpenShow(RecentShow show)
        {
            ShowContext.DatabaseState state;
            using (var context = ShowContext.Open(show))
            {
                using (var loading = new LoadingOverlay(this).AsSegment(nameof(StartWindow) + nameof(OpenShow)))
                {
                    //TODO handle server connection exceptions
                    using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.PreloadModel), "Preparing show model"))
                        await context.PreloadModel(); // do this here while the overlay is shown to avoid a synchronous delay when the MainMenu is loaded
                    using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.CheckDatabaseState), "Checking database integrity"))
                        state = await context.CheckDatabaseState();
                }
                if (state == ShowContext.DatabaseState.Empty)
                {
                    using (new LoadingOverlay(this) { SubText = "Creating new database" })
                        await context.CreateNewDatabase(show.DefaultShowName);
                }
                else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
                {
                    MessageBox.Show("This database was saved with a newer version of CARMEN and cannot be opened. Please install the latest version.", "CARMEN");
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
                {
                    if (MessageBox.Show("This database was saved with an older version of CARMEN, would you like to upgrade it?\nNOTE: Once upgraded, older versions will no longer be able to read this database.", "CARMEN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (new LoadingOverlay(this) { SubText = "Upgrading database" })
                        {
                            show.CreateBackupIfFile();
                            await context.UpgradeDatabase();
                        }
                    }
                    else
                        return;
                }
            }
            using (new LoadingOverlay(this) { SubText = "Opening show" })
                LaunchMainWindow(show);
        }

        private static void AddToRecentList(RecentShow show)
        {
            show.LastOpened = DateTime.Now;
            var recent = Properties.Settings.Default.RecentShows;
            recent.Remove(show);
            recent.Insert(0, show);
            while (recent.Count > MAX_RECENT_SHOWS)
                recent.RemoveAt(recent.Count - 1);
        }

        private static void RemoveFromRecentList(RecentShow show)
        {
            var recent = Properties.Settings.Default.RecentShows;
            recent.Remove(show);
        }

        private void LaunchMainWindow(RecentShow show)
        {
            AddToRecentList(show);
            var main = new MainWindow(show);
            main.Show();
            this.Close();
        }

        private async void RecentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0 && e.AddedItems[0] is RecentShow show)
            {
                if (show.CheckAssessible())
                    await OpenShow(show);
                else
                {
                    if (MessageBox.Show($"{show.Label} cannot be accessed. Would you like to remove it from the recent shows list?", "CARMEN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        RemoveFromRecentList(show);
                    RecentList.Items.Refresh();
                }
                RecentList.SelectedItem = null;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Properties.Settings.Default.OpenSettingsOnF1 && e.Key == Key.F1 && Keyboard.Modifiers == ModifierKeys.None)
            {
                SettingsButton_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.ShortcutsOnStartWindow && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.N)
                    NewButton_Click(sender, e);
                else if (e.Key == Key.O)
                    OpenButton_Click(sender, e);
                else if (e.Key == Key.N)
                    ConnectButton_Click(sender, e);
                else if (e.Key.IsDigit(out var digit) && digit > 0 && digit <= RecentList.Items.Count)
                    RecentList_SelectionChanged(sender, new(e.RoutedEvent, Array.Empty<object>(), new[] { RecentList.Items[digit.Value - 1] }));
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var show = new RecentShow { Provider = DbProvider.MySql };
            var login = new LoginDialog(show)
            {
                Owner = this
            };
            if (login.ShowDialog() == true)
                await OpenShow(show);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Closing += Settings_Closing;
            settings.Show();
            this.Close();
        }

        private void Settings_Closing(object? sender, EventArgs e)
        {
            var new_start_window = new StartWindow();
            new_start_window.Show();
        }
    }
}

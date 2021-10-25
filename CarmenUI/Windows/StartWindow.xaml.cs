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
                var show = RecentShow.FromLocalFile(dialog.FileName);
                using var loading = new LoadingOverlay(this).AsSegment(nameof(StartWindow) + nameof(NewButton_Click));
                using (loading.Segment(nameof(StartWindow) + nameof(CreateNewShow), "Creating new database"))
                    await CreateNewShow(show);
                using (loading.Segment(nameof(StartWindow) + nameof(AddToRecentList), "Adding to recents list"))
                    AddToRecentList(show);
                using (loading.Segment(nameof(StartWindow) + nameof(LaunchMainWindow), "Opening show"))
                    LaunchMainWindow(show);
            }
        }

        private static async Task CreateNewShow(RecentShow show)
        {
            using (var context = new ShowContext(show))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
#if DEBUG
                if (MessageBox.Show("Do you want to add test data?", "DEBUG", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                else
                {
                    context.SetDefaultShowSettings(show.DefaultShowName);
                }
#else
                context.SetDefaultShowSettings(show.DefaultShowName);
#endif
                await context.SaveChangesAsync();
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Existing Database",
                Filter = "Sqlite Database (*.db)|*.db|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                OpenShow(RecentShow.FromLocalFile(dialog.FileName));
        }

        private void OpenShow(RecentShow show)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(StartWindow) + nameof(OpenShow));
            using (loading.Segment(nameof(StartWindow) + nameof(CheckIntegrity), "Checking database integrity"))
                CheckIntegrity(show);
            using (loading.Segment(nameof(StartWindow) + nameof(AddToRecentList), "Adding to recents list"))
                AddToRecentList(show);
            using (loading.Segment(nameof(StartWindow) + nameof(LaunchMainWindow), "Opening show"))
                LaunchMainWindow(show);
        }

        private static void CheckIntegrity(ShowConnection connection)
        {
            using (var context = new ShowContext(connection))
            {
                // Accessing any model or entity related property on the context causes the model to be build, which takes ~1s synchronously.
                // It is important to do this here, while the loading form is shown, to avoid a synchronous delay when the MainMenu is loaded.
                _ = context.Model;
            }
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
            var main = new MainWindow(show);
            main.Show();
            this.Close();
        }

        private void RecentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0 && e.AddedItems[0] is RecentShow show)
            {
                if (show.IsAssessible)
                    OpenShow(show);
                else
                {
                    if (MessageBox.Show($"{show.Label} cannot be accessed. Would you like to remove it from the recent shows list?", "CARMEN", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        RemoveFromRecentList(show);
                    RecentList.SelectedItem = null;
                    RecentList.Items.Refresh();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Properties.Settings.Default.OpenSettingsOnF1 && e.Key == Key.F1)
            {
                SettingsButton_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.ShortcutsOnStartWindow)
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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Not yet implemented
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

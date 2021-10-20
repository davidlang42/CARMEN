using CarmenUI.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                var show = RecentShow.FromLocalFile(dialog.FileName);
                var options = show.CreateOptions();
                using (var loading = new LoadingOverlay(this))
                {
                    using (var context = new ShowContext(options))
                    {
                        //TODO handle io errors
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
#if DEBUG
                        if (MessageBox.Show("Do you want to add test data?", "DEBUG", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            using var test_data = new TestDataGenerator(context, 0);
                            test_data.AddAlternativeCasts();
                            test_data.AddCastGroups(4);
                            context.SaveChanges();
                            test_data.AddShowStructure(30, 6, 1, include_items_at_every_depth: false); // after cast groups committed
                            context.SaveChanges();
                            test_data.AddCriteriaAndRequirements(); // after cast groups committed
                            context.SaveChanges();
                            test_data.AddTags(1); // after requirements committed
                            context.SaveChanges();
                            test_data.AddApplicants(100); // after criteria, tags, alternative casts committed
                            context.SaveChanges();
                            test_data.AddRoles(5); // after applicants, items, cast groups, requirements committed
                            test_data.AddImages(); // after applicants, tags committed
                            context.SaveChanges();
                        }
                        else
                        {
                            context.SetDefaultShowSettings(show.DefaultShowName);
                        }
#else
                        context.SetDefaultShowSettings(show.DefaultShowName);
#endif
                        context.SaveChanges();
                    }
                    AddToRecentList(show);
                    LaunchMainWindow(options, show);
                }
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
            {
                var show = RecentShow.FromLocalFile(dialog.FileName);
                var options = show.CreateOptions();
                using (var loading = new LoadingOverlay(this))
                {
                    CheckIntegrity(options);
                    AddToRecentList(show);
                    LaunchMainWindow(options, show);
                }
            }
        }

        private void CheckIntegrity(DbContextOptions<ShowContext> options)
        {
            using (var context = new ShowContext(options))
            {
                // Accessing any model or entity related property on the context causes the model to be build, which takes ~1s synchronously.
                // It is important to do this here, while the loading form is shown, to avoid a synchronous delay when the MainMenu is loaded.
                _ = context.Model;
                //TODO handle io errors
                //TODO ensure that db matches schema
            }
        }

        private void AddToRecentList(RecentShow show)
        {
            var recent = Properties.Settings.Default.RecentShows;
            recent.Remove(show);
            recent.Insert(0, show);
            while (recent.Count > MAX_RECENT_SHOWS)
                recent.RemoveAt(recent.Count - 1);
        }

        private void LaunchMainWindow(DbContextOptions<ShowContext> options, RecentShow show)
        {
            var main = new MainWindow(options, show.Label, show.DefaultShowName);
            main.Show();
            this.Close();
        }

        private void RecentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0 && e.AddedItems[0] is RecentShow show)
            {
                using (var loading = new LoadingOverlay(this))
                {
                    var options = show.CreateOptions();
                    CheckIntegrity(options);
                    show.LastOpened = DateTime.Now;
                    AddToRecentList(show);
                    LaunchMainWindow(options, show);
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

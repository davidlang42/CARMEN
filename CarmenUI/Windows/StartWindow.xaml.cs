﻿using CarmenUI.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ShowModel;
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
                        //LATER handle io errors
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
#if DEBUG
                        if (MessageBox.Show("Do you want to add test data?","DEBUG",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                            test_data.AddImages(); // after applicants, cast groups, tags committed
                            context.SaveChanges();
                        }
#endif
                        context.ShowRoot.Name = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                        context.SaveChanges();
                    }
                    AddToRecentList(show);
                    LaunchMainWindow(options, show.Label);
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
                    LaunchMainWindow(options, show.Label);
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
                //LATER handle io errors
                //LATER ensure that db matches schema
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

        private void LaunchMainWindow(DbContextOptions<ShowContext> options, string label)
        {
            var main = new MainWindow(options, label);
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
                    LaunchMainWindow(options, show.Label);
                }
            }
        }
    }
}

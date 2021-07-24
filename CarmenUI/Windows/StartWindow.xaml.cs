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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
            => MessageBox.Show("Not yet implemented, please choose another option."); //TODO implement connect to db

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                var connection = FileConnection(dialog.FileName);
                //TODO show loading screen before main window (use using)
                using (var context = new ShowContext(connection))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();//TODO handle io errors
                    context.ShowRoot.Name = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    context.SaveChanges();
                }
                LaunchMainWindow(connection);
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
                var connection = FileConnection(dialog.FileName);
                //TODO show loading screen before main window (use using)
                using (var context = new ShowContext(connection))
                {
                    //TODO handle io errors
                    //TODO ensure that db matches schema
                }
                LaunchMainWindow(connection);
            }
        }

        private ShowConnection FileConnection(string filename)
            => new ShowConnection
            {
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString(),
                Label = System.IO.Path.GetFileName(filename)
            };

        private void LaunchMainWindow(ShowConnection connection)
        {
            var recent = Properties.Settings.Default.RecentShows;
            recent.Remove(connection);
            recent.Insert(0, connection);
            while (recent.Count > MAX_RECENT_SHOWS)
                recent.RemoveAt(recent.Count - 1);
            var main = new MainWindow(connection);
            main.Show();
            this.Close();
        }
    }
}

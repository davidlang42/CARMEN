using Carmen.ShowModel.Applicants;
using Carmen.Desktop.Converters;
using Serilog;
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
using Carmen.Desktop.ViewModels;
using MySqlConnector;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Carmen.ShowModel;

namespace Carmen.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for ApplicantPickerDialog.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
        //readonly CollectionViewSource applicantsViewSource;
        readonly string connectionString;
        readonly string databaseName;
        readonly string currentUser;

        public UsersWindow(RecentShow server)
        {
            Log.Information(nameof(UsersWindow));
            if (server.Provider != DbProvider.MySql) {
                throw new UserException("User management is only available for MySql servers");
            }
            InitializeComponent();
            connectionString = server.ConnectionString;
            databaseName = server.Database;
            currentUser = server.User;
            RefreshList();
        }

        void RefreshList()
        {
            UserList.ItemsSource = QueryUsers();
        }

        MySqlConnection OpenDatabase()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        DatabaseUser[] QueryUsers()
        {
            using var connection = OpenDatabase();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT User, Host, db FROM mysql.db WHERE @database LIKE db;";
            cmd.Parameters.AddWithValue("@database", databaseName);
            using var res = cmd.ExecuteReader();
            var users = new List<DatabaseUser>();
            while (res.Read()) {
                var name = res.GetString("User");
                var host = res.GetString("Host");
                var grants = QueryGrants(name, host);
                var db = res.GetString("db");
                users.Add(new(name, host, db, databaseName, grants));
            }
            return users.ToArray();
        }

        string[] QueryGrants(string name, string host)
        {
            using var connection = OpenDatabase();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SHOW GRANTS FOR @user@@host;";
            cmd.Parameters.AddWithValue("@user", name);
            cmd.Parameters.AddWithValue("@host", host);
            using var res = cmd.ExecuteReader();
            var grants = new List<string>();
            while (res.Read())
            {
                grants.Add(res.GetString(0));
            }
            return grants.ToArray();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            //TODO prompt for user & password
            //TODO CREATE USER 'USERNAME'@'%' IDENTIFIED BY 'PASSWORD' REQUIRE SSL;
            //TODO user.GrantRead();
            RefreshList();
        }

        private void GrantWrite_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user)
            {
                throw new UserException("Please select a user"); //TODO make disabled if no selection
            }
            //TODO confirm grant
            user.GrantWrite();
            RefreshList();
        }

        private void GrantAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user)
            {
                throw new UserException("Please select a user"); //TODO make disabled if no selection
            }
            //TODO confirm grant
            user.GrantAdmin();
            RefreshList();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user) {
                throw new UserException("Please select a user"); //TODO make disabled if no selection
            }
            if (user.Name == currentUser) {
                MessageBox.Show("You cannot delete the user you are currently logged in as.");
                return;
            }
            //TODO confirm delete
            //TODO DROP USER 'USERNAME'@'%';
            RefreshList();
        }
    }
}

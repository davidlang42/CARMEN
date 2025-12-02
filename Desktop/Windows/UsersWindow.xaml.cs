using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using Carmen.Desktop.ViewModels;
using MySqlConnector;
using Carmen.ShowModel;

namespace Carmen.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
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
            UserList.ItemsSource = QueryUsers(AllUsersCheckBox.IsChecked ?? false);
        }

        MySqlConnection OpenDatabase()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        DatabaseUser[] QueryUsers(bool include_all_users)
        {
            using var connection = OpenDatabase();
            var cmd = connection.CreateCommand();
            string? effective_db;
            if (include_all_users) {
                cmd.CommandText = $"SELECT User, Host, db FROM mysql.db WHERE db <> 'mysql' ORDER BY User;";
                effective_db = null;
            } else {
                cmd.CommandText = $"SELECT User, Host, db FROM mysql.db WHERE @database LIKE db ORDER BY User;";
                cmd.Parameters.AddWithValue("@database", databaseName);
                effective_db = databaseName;
            }
            using var res = cmd.ExecuteReader();
            var users = new List<DatabaseUser>();
            while (res.Read()) {
                var name = res.GetString("User");
                var host = res.GetString("Host");
                var grants = QueryGrants(name, host);
                var db = res.GetString("db");
                users.Add(new(name, host, db, effective_db, grants));
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

        void ExecuteGrantSql(string sql)
        {
            try {
                using var connection = OpenDatabase();
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                MessageBox.Show($"Error while granting access: {ex.InnermostException().Message}\nAccess not granted.");
            }
        }

        DatabaseUser? AddUser(string name, string password, string host)
        {
            try {
                using var connection = OpenDatabase();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE USER @user@@host IDENTIFIED BY @password REQUIRE SSL;";
                cmd.Parameters.AddWithValue("@user", name);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@host", host);
                cmd.ExecuteNonQuery();
                return new DatabaseUser(name, host, databaseName, databaseName, Array.Empty<string>());
            } catch (Exception ex) {
                MessageBox.Show($"Error while adding user: {ex.InnermostException().Message}\nUser not added.");
                return null;
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Username?", "Add user"); // I'm sorry
            if (string.IsNullOrEmpty(name)) {
                return;
            }
            var password = Microsoft.VisualBasic.Interaction.InputBox("Password?", "Add user"); // I'm sorry
            if (string.IsNullOrEmpty(password)) {
                return;
            }
            if (AddUser(name, password, "%") is DatabaseUser user) {
                ExecuteGrantSql(user.SqlToGrantRead());
            }
            RefreshList();
        }

        private void GrantRead_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user) {
                throw new UserException("Please select a user");
            }
            var msg = $"Are you sure you want to grant user '{user.Name}' read access to '{databaseName}'?";
            if (MessageBox.Show(msg, "Grant read access", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) {
                return;
            }
            ExecuteGrantSql(user.SqlToGrantRead(databaseName));
            RefreshList();
        }

        private void GrantWrite_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user) {
                throw new UserException("Please select a user");
            }
            var msg = $"Are you sure you want to grant user '{user.Name}' write access to '{databaseName}'?";
            if (MessageBox.Show(msg, "Grant write access", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) {
                return;
            }
            ExecuteGrantSql(user.SqlToGrantWrite(databaseName));
            RefreshList();
        }

        private void GrantAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user) {
                throw new UserException("Please select a user");
            }
            var msg = $"Are you sure you want to grant user '{user.Name}' ADMIN access?";
            if (user.Database != databaseName) {
                msg += "\nNOTE: This will allow this user to manage users for any database they have access to.";
            }
            if (MessageBox.Show(msg, "Grant admin access", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) {
                return;
            }
            foreach (var sql in user.SqlToGrantAdmin()) {
                ExecuteGrantSql(sql);
            }
            RefreshList();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is not DatabaseUser user) {
                throw new UserException("Please select a user");
            }
            if (user.Name == currentUser) {
                MessageBox.Show("You cannot delete the user you are currently logged in as.");
                return;
            }
            if (MessageBox.Show($"Are you sure you want to delete user '{user.Name}'?\nNOTE: This will delete this user for all databases.", "Delete user", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) {
                return;
            }
            ExecuteGrantSql(user.SqlToDeleteUser());
            RefreshList();
        }

        private void AllUsersCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }
    }
}

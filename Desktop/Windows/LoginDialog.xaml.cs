using Carmen.Desktop.ViewModels;
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

namespace Carmen.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        RecentShow show;

        public LoginDialog(RecentShow show)
        {
            Log.Information(nameof(LoginDialog));
            this.show = show;
            InitializeComponent();
            MainGrid.DataContext = show;
            PasswordText.Password = show.Password;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            show.Password = PasswordText.Password;
            if (!show.CheckAssessible())
            {
                MessageBox.Show("Server not accessible.", Title);
                return;
            }
            if (!show.TryConnection(out var error))
            {
                MessageBox.Show($"Connection error: {error}", Title);
                return;
            }
            DialogResult = true;
        }
    }
}

using CarmenUI.ViewModels;
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
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        RecentShow show;

        public LoginDialog(RecentShow show)
        {
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
                MessageBox.Show("Failed to connect.");
                return;
            }
            DialogResult = true;
        }
    }
}

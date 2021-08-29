using CarmenUI.Pages;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionLabel;

        public MainWindow(DbContextOptions<ShowContext> context_options, string connection_label, string default_show_name)
        {
            if (Properties.Settings.Default.SelectAllOnFocusTextBox)
                EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus)); //LATER does this need to undo on unload?
            InitializeComponent();
            connectionLabel = connection_label;
            var main_menu = new MainMenu(context_options, connection_label, default_show_name);
            MainFrame.Navigate(main_menu);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
                Title = $"CARMEN: {page.Title} ({connectionLabel})";
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }
    }
}

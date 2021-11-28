using CarmenUI.Pages;
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
using CarmenUI.ViewModels;
using Serilog;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RecentShow connection;

        public MainWindow(RecentShow connection)
        {
            Log.Information(nameof(MainWindow));
            if (Properties.Settings.Default.SelectAllOnFocusTextBox)
                EventManager.RegisterClassHandler(typeof(TextBox), GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
            InitializeComponent();
            NavigationCommands.BrowseBack.InputGestures.Clear(); // otherwise the backspace key changes pages without saving or confirming
            NavigationCommands.BrowseForward.InputGestures.Clear();
            this.connection = connection;
            NavigateToMainMenu();
        }

        public void NavigateToMainMenu()
        {
            var main_menu = new MainMenu(connection);
            MainFrame.Navigate(main_menu);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
                Title = $"CARMEN: {page.Title} ({connection.Label})";
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && Properties.Settings.Default.ReportOnCtrlR)
            {
                OpenReport(null);
                e.Handled = true;
            }
        }

        private uint reportCount = 1;
        public void OpenReport(ReportDefinition? definition)
        {
            if (reportCount == uint.MaxValue)
                reportCount = 1;
            var report = new ReportWindow(connection, $"Report #{reportCount++}", definition)
            {
                Owner = this
            };
            report.Show();
            Log.Information($"Opened report: {definition?.SavedName ?? reportCount.ToString()}");
        }
    }
}

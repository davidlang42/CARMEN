using CarmenUI.Pages;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Structure;
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
        public ShowContext context;
        string connectionLabel;

        public MainWindow(DbContextOptions<ShowContext> context_options, string connection_label)
        {
            InitializeComponent();
            context = new ShowContext(context_options);
            connectionLabel = connection_label;
            var main_menu = new MainMenu(context);
            MainFrame.Navigate(main_menu);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            context.Dispose();
            base.OnClosing(e);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
                Title = $"CARMEN: {page.Title} ({connectionLabel})";
        }
    }
}

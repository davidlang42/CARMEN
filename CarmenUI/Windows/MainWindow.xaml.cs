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
        ShowContext context;

        public MainWindow(DbContextOptions<ShowContext> context_options)
        {
            InitializeComponent();
            context = new ShowContext(context_options);
            Title = $"CARMEN: {context.ShowRoot.Name}"; //TODO should be bound: Title="{MultiBinding StringFormat='CARMEN: {0}', Bindings={Binding Name}}"
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            context.Dispose();
            base.OnClosing(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}

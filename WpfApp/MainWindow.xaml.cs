using Microsoft.EntityFrameworkCore;
using Model;
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

namespace App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ShowContext _context = new ShowContext();

        private CollectionViewSource sectionViewSource;
        public MainWindow()
        {
            InitializeComponent();
            sectionViewSource = (CollectionViewSource)FindResource(nameof(sectionViewSource));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _context.Database.EnsureCreated(); // for demo purposes
            _context.Sections.Load(); // load the entities into EF Core
            sectionViewSource.Source = _context.Sections.Local.ToObservableCollection();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _context.SaveChanges(); // all changes were tracked, including deletes
            sectionsDataGrid.Items.Refresh();
            itemsDataGrid.Items.Refresh();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _context.Dispose();
            base.OnClosing(e);
        }
    }
}

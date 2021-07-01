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
        private ShowContext _context = new ShowContext();

        private readonly CollectionViewSource sectionsViewSource;

        public MainWindow()
        {
            InitializeComponent();
            sectionsViewSource = (CollectionViewSource)FindResource(nameof(sectionsViewSource));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _context.Database.EnsureCreated(); // for demo purposes
            PopulateViews();
        }

        private void PopulateViews()
        {
            _context.Sections.Load(); // load the entities into EF Core
            sectionsViewSource.Source = _context.Sections.Local.ToObservableCollection();
        }

        private void RefreshViews()
        {
            sectionsDataGrid.Items.Refresh();
            itemsDataGrid.Items.Refresh();
            rolesDataGrid.Items.Refresh();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _context.SaveChanges(); // all changes were tracked, including deletes
            RefreshViews();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _context.Dispose();
            _context = new ShowContext();
            PopulateViews();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _context.Dispose();
            base.OnClosing(e);
        }
    }
}

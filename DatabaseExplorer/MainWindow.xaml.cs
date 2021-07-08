using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Model;
using Model.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Section = Model.Structure.Section;

namespace App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShowContext? _context;

        public ShowContext Context => _context ?? throw new Exception("Attempted to use Context before loading.");

        /// <summary>Create a new database context, disposing the old one if set</summary>
        private void CreateContext(string connection_string)
        {
            _context?.Dispose();
            var context_options = new DbContextOptionsBuilder<ShowContext>().UseSqlite(connection_string).Options;
            _context = new ShowContext(context_options);
        }

        private readonly CollectionViewSource applicantsViewSource;
        private readonly CollectionViewSource castGroupsViewSource;
        private readonly CollectionViewSource rootNodesViewSource;
        private readonly CollectionViewSource criteriaViewSource;
        private readonly CollectionViewSource imagesViewSource;
        private readonly CollectionViewSource sectionTypesViewSource;

        public MainWindow()
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            criteriaViewSource = (CollectionViewSource)FindResource(nameof(criteriaViewSource));
            imagesViewSource = (CollectionViewSource)FindResource(nameof(imagesViewSource));
            sectionTypesViewSource = (CollectionViewSource)FindResource(nameof(sectionTypesViewSource));
        }

        private void CreateMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName);
                Context.Database.EnsureDeleted();
                Context.Database.EnsureCreated();
                PopulateViews();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Existing Database",
                Filter = "Sqlite Database (*.db)|*.db|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName);
                PopulateViews();
            }
        }

        private void OpenDatabase(string filename)
        {
            CreateContext($"Data Source={filename}"); //TODO unsafe injection
            foreach (var ui_element in this.AllControls<UIElement>())
                ui_element.IsEnabled = true;
        }

        private void PopulateViews()
        {
            // Load the entities into EF Core
            Context.Applicants.Load();
            Context.CastGroups.Load();
            Context.Nodes.Load();
            Context.Criteria.Load();
            Context.Images.Load();
            Context.SectionTypes.Load();
            // Put collections into view source
            applicantsViewSource.Source = Context.Applicants.Local.ToObservableCollection();
            castGroupsViewSource.Source = Context.CastGroups.Local.ToObservableCollection();
            rootNodesViewSource.Source = Context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(new SortDescription(nameof(IOrdered.Order), ListSortDirection.Ascending)); // sorts top level only, other levels sorted by SortIOrdered converter
            criteriaViewSource.Source = Context.Criteria.Local.ToObservableCollection();
            imagesViewSource.Source = Context.Images.Local.ToObservableCollection();
            sectionTypesViewSource.Source = Context.SectionTypes.Local.ToObservableCollection();
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Context.SaveChanges();
                RefreshViews();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Could not save changes: \n" + ex.Message);
                RevertChanges();
            }
        }

        /// <summary>Updates all ItemControls with data generated by DB commit, eg. auto-ids, cascaded-deletes</summary>
        private void RefreshViews()
        {
            foreach (var items_control in this.AllControls<ItemsControl>())
                items_control.Items.Refresh();
        }

        private void RevertMenu_Click(object sender, RoutedEventArgs e) => RevertChanges();

        private void RevertChanges()
        {
            CreateContext(Context.Database.GetConnectionString());
            PopulateViews();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e) => Close();

        private void TestDataMenu_Click(object sender, RoutedEventArgs e)//TODO make a better test data generator
        {
            var show = Context.ShowRoot;
            var s1 = new Section
            {
                Name = "Section 1",
                Parent = show
            };
            var s2 = new Section
            {
                Name = "Section 2",
                Parent = show
            };
            var i3 = new Item
            {
                Name = "Item 3",
                Parent = show
            };
            var i1 = new Item
            {
                Name = "Item 1a"
            };
            var i2 = new Item
            {
                Name = "Item 2a",
            };
            s1.Children.Add(i1);
            s2.Children.Add(i2);
            Context.AddRange(s1, s2, i3);
            Context.SaveChanges();
            RefreshViews();
        }

        private void ClearDataMenu_Click(object sender, RoutedEventArgs e)
        {
            Context.Applicants.RemoveRange(Context.Applicants);
            Context.CastGroups.RemoveRange(Context.CastGroups);
            Context.Nodes.RemoveRange(Context.Nodes);
            Context.Criteria.RemoveRange(Context.Criteria);
            Context.Images.RemoveRange(Context.Images);
            Context.SectionTypes.RemoveRange(Context.SectionTypes);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Context.Dispose();
            base.OnClosing(e);
        }
    }
}

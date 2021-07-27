using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Structure;
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

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for AllocateRoles.xaml
    /// </summary>
    public partial class AllocateRoles : SubPage
    {
        private readonly CollectionViewSource rootNodesViewSource;

        public AllocateRoles(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            context.Nodes.Load(); //TODO load async
            rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(new SortDescription(nameof(IOrdered.Order), ListSortDirection.Ascending)); // sorts top level only, other levels sorted by SortIOrdered converter
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(DataObjects.Applicants | DataObjects.Nodes);
        }

        private void ItemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ItemsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ItemsTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ItemsTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ItemsTreeView_Drop(object sender, DragEventArgs e)
        {

        }

        private void RolesTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void RolesTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void RolesTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void RolesTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void RolesTreeView_Drop(object sender, DragEventArgs e)
        {

        }

        private void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

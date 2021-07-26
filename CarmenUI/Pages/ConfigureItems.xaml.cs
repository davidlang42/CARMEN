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
    /// Interaction logic for ConfigureItems.xaml
    /// </summary>
    public partial class ConfigureItems : PageFunction<bool>
    {
        private readonly CollectionViewSource rootNodesViewSource;

        public ConfigureItems(ObservableCollection<Node> nodes)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            rootNodesViewSource.Source = nodes;
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(new SortDescription(nameof(IOrdered.Order), ListSortDirection.Ascending)); // sorts top level only, other levels sorted by SortIOrdered converter
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(new ReturnEventArgs<bool>(true));
        }

        private void ItemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ItemsTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ItemsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ItemsTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ItemsTreeView_Drop(object sender, DragEventArgs e)
        {

        }
    }
}

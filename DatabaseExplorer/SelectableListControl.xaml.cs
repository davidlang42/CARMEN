using Model.Requirements;
using System;
using System.Collections;
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

namespace DatabaseExplorer
{
    /// <summary>
    /// Interaction logic for SelectableListControl.xaml
    /// </summary>
    public partial class SelectableListControl : UserControl
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems), typeof(ICollection<Requirement>), typeof(SelectableListControl), new PropertyMetadata(null));

        public static readonly DependencyProperty AvailableItemsProperty = DependencyProperty.Register(
            nameof(AvailableItems), typeof(IEnumerable), typeof(SelectableListControl), new PropertyMetadata(null));

        public ICollection<Requirement>? SelectedItems //TODO SelectedList doesn't remove items when they are deleted
        {
            get => (ICollection<Requirement>?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public IEnumerable? AvailableItems
        {
            get => (IEnumerable?)GetValue(AvailableItemsProperty);
            set => SetValue(AvailableItemsProperty, value);
        }

        public SelectableListControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var requirement in availableList.SelectedItems.OfType<Requirement>())
                if (!SelectedItems.Contains(requirement))
                    SelectedItems.Add(requirement);
        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var requirement in availableList.Items.OfType<Requirement>())
                if (!SelectedItems.Contains(requirement))
                    SelectedItems.Add(requirement);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var requirement in selectedList.SelectedItems.OfType<Requirement>().ToList())
                if (SelectedItems.Contains(requirement))
                    SelectedItems.Remove(requirement);
        }

        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var requirement in selectedList.Items.OfType<Requirement>().ToList())
                if (SelectedItems.Contains(requirement))
                    SelectedItems.Remove(requirement);
        }

        private void availableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addButton_Click(sender, new RoutedEventArgs());

        private void selectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeButton_Click(sender, new RoutedEventArgs());
    }
}

using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DatabaseExplorer
{
    /// <summary>
    /// Interaction logic for SelectableListControl.xaml
    /// </summary>
    public partial class SelectableListControl : UserControl
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems), typeof(IList), typeof(SelectableListControl), new PropertyMetadata(null));

        public static readonly DependencyProperty AvailableItemsProperty = DependencyProperty.Register(
            nameof(AvailableItems), typeof(IEnumerable), typeof(SelectableListControl), new PropertyMetadata(null));

        public IList? SelectedItems //TODO SelectedList doesn't remove items when they are deleted
        {
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public IEnumerable? AvailableItems //TODO filter selected items out of availableList 
        {
            get => (IEnumerable?)GetValue(AvailableItemsProperty);
            set => SetValue(AvailableItemsProperty, value);
        }

        public string ItemDisplayMemberPath { get; set; } = "";

        public string Title { get; set; } = "";

        public SelectableListControl() //TODO implement drag & drop between selectedList and availableList
        {
            InitializeComponent();
            DataContext = this;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var item in availableList.SelectedItems)
                if (!SelectedItems.Contains(item))
                    SelectedItems.Add(item);
        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var item in availableList.Items)
                if (!SelectedItems.Contains(item))
                    SelectedItems.Add(item);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var item in selectedList.SelectedItems.OfType<object>().ToList())
                if (SelectedItems.Contains(item))
                    SelectedItems.Remove(item);
        }

        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItems == null)
                return;
            foreach (var item in selectedList.Items.OfType<object>().ToList())
                if (SelectedItems.Contains(item))
                    SelectedItems.Remove(item);
        }

        private void availableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addButton_Click(sender, new RoutedEventArgs());

        private void selectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeButton_Click(sender, new RoutedEventArgs());
    }
}

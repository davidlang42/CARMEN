using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for EditableListControl.xaml
    /// </summary>
    public partial class EditableListControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IList), typeof(EditableListControl), new PropertyMetadata(null));

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        //TODO add a parameter for title of list

        public EditableListControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void optionsMoveUp_Click(object sender, RoutedEventArgs e)
        {
            //TODO implement move up
        }

        private void optionsDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null || optionsList.SelectedItem == null)
                return;
            if (ItemsSource.IsFixedSize || ItemsSource.IsReadOnly)
                ItemsSource = Enumerable.Range(0, ItemsSource.Count).Zip(ItemsSource.Cast<string>())
                    .Where(p => p.First != optionsList.SelectedIndex).Select(p => p.Second).ToArray();
            else
                ItemsSource.RemoveAt(optionsList.SelectedIndex);
            //TODO select item at removed index
        }

        private void optionsMoveDown_Click(object sender, RoutedEventArgs e)
        {
            //TODO implement move down
        }

        private void optionsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null)
                return;
            if (ItemsSource.IsFixedSize || ItemsSource.IsReadOnly)
                ItemsSource = ItemsSource.Cast<string>().Concat(new[] { optionsNewItem.Text }).ToArray(); //TODO insert before selected item (if selected), that way when something is double clicked to edit, and then you hit enter, it goes back where it was
            else
                ItemsSource.Add(optionsNewItem.Text);
            optionsNewItem.Text = "";
        }

        private void optionsNewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                optionsAdd_Click(sender, new RoutedEventArgs());
        }

        private void optionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (optionsList.SelectedItem == null)
                return;
            optionsNewItem.Text = optionsList.SelectedItem.ToString();
            optionsDelete_Click(sender, new RoutedEventArgs());
            optionsNewItem.Focus();
        }
    }
}

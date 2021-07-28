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

namespace CarmenUI.UserControls
{
    /// <summary>
    /// Interaction logic for EditableListControl.xaml
    /// </summary>
    public partial class EditableList : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IList), typeof(EditableList), new PropertyMetadata(null));

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string Title { get; set; } = "";

        public EditableList()
        {
            InitializeComponent();
            DataContext = this; //TODO this causes a normal {Binding Options} to not work on ItemsSource, see if there is a way to do this without setting DataContext
        }

        private void optionsMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (optionsList.SelectedItem != null && optionsList.SelectedIndex != 0)
                MoveItem(optionsList.SelectedIndex, optionsList.SelectedIndex - 1);
        }

        private void optionsDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null || optionsList.SelectedItem == null)
                return;
            var index = optionsList.SelectedIndex;
            using (var list = new EditableListWrapper<string>(ItemsSource, new_list => ItemsSource = new_list))
                list.RemoveAt(index);
            optionsList.SelectedIndex = index;
            optionsNewItem.Focus();
        }

        private void optionsMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (optionsList.SelectedItem != null && optionsList.SelectedIndex != optionsList.Items.Count - 1)
                MoveItem(optionsList.SelectedIndex, optionsList.SelectedIndex + 1);
        }

        private void MoveItem(int from_index, int to_index)
        {
            if (ItemsSource == null)
                return;
            var value = optionsList.Items[from_index];
            using (var list = new EditableListWrapper<string>(ItemsSource, new_list => ItemsSource = new_list))
            {
                list.RemoveAt(from_index);
                list.Insert(to_index, value);
            }
            optionsList.SelectedIndex = to_index;
        }

        private void optionsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null)
                return;
            var value = optionsNewItem.Text;
            using (var list = new EditableListWrapper<string>(ItemsSource, new_list => ItemsSource = new_list))
                if (optionsList.SelectedIndex < 0)
                    list.Add(value);
                else
                    list.Insert(optionsList.SelectedIndex, value);
            optionsNewItem.Text = "";
            optionsNewItem.Focus();
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
            optionsNewItem.SelectAll();
        }

        private void optionsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                optionsDelete_Click(sender, new RoutedEventArgs());
        }
    }

    public class EditableListWrapper<T> : IList, IDisposable
    {
        private IList workingList;
        private Action? setter;

        public EditableListWrapper(IList source_list, Action<IList> setter)
        {
            if (source_list.IsReadOnly || source_list.IsFixedSize)
            {
                // list will be replaced on dispose
                var new_list = source_list.OfType<T>().ToList();
                workingList = new_list;
                this.setter = () => setter(new_list.ToArray());
            }
            else
            {
                // list will be modified directly
                workingList = source_list;
                this.setter = null;
            }
        }

        #region Implement IList through workingList
        public object? this[int index] { get => workingList[index]; set => workingList[index] = value; }
        public bool IsFixedSize => workingList.IsFixedSize;
        public bool IsReadOnly => workingList.IsReadOnly;
        public int Count => workingList.Count;
        public bool IsSynchronized => workingList.IsSynchronized;
        public object SyncRoot => workingList.SyncRoot;
        public int Add(object? value) => workingList.Add(value);
        public void Clear() => workingList.Clear();
        public bool Contains(object? value) => workingList.Contains(value);
        public void CopyTo(Array array, int index) => workingList.CopyTo(array, index);
        public IEnumerator GetEnumerator() => workingList.GetEnumerator();
        public int IndexOf(object? value) => workingList.IndexOf(value);
        public void Insert(int index, object? value) => workingList.Insert(index, value);
        public void Remove(object? value) => workingList.Remove(value);
        public void RemoveAt(int index) => workingList.RemoveAt(index);
        #endregion

        public void Dispose()
        {
            setter?.Invoke();
            setter = null;
        }
    }
}

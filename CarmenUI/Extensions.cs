using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarmenUI
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Select(u => (int)u).Sum();

        /// <summary>Like SingleOrDefault() except returns null if more than one element rather than throwing an exception</summary>
        public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> list) where T : class
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                return null;
            var first = e.Current;
            if (e.MoveNext())
                return null;
            return first;
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> range)
        {
            foreach (var item in range)
                set.Add(item);
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
                collection.Add(item);
        }

        public static bool AllEqual<T>(this IEnumerable<T> sequence)
            where T : notnull
        {
            var e = sequence.GetEnumerator();
            if (!e.MoveNext())
                return true;
            var first = e.Current;
            while (e.MoveNext())
                if (!e.Current.Equals(first))
                    return false;
            return true;
        }
    }

    internal static class AsyncExtensions
    {
        public static Task<int> CountAsync<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            => Task.Run(() => collection.Count(predicate));

        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> collection)
            => Task.Run(() => collection.ToList());

        public static Task<uint> SumAsync<T>(this IEnumerable<T> collection, Func<T, uint> selector)
            => Task.Run(() => (uint)collection.Sum(v => selector(v)));

        public static Task<Dictionary<U, V>> ToDictionaryAsync<T, U, V>(this IEnumerable<T> collection, Func<T, U> key_selector, Func<T, V> value_selector) where U : notnull
            => Task.Run(() => collection.ToDictionary(key_selector, value_selector));
    }

    internal static class WpfExtensions
    {
        /// <summary>Collapses all children of the panel, except the given child, which is made visible.</summary>
        public static void ShowOneChild(this Panel panel, UIElement visible_child)//TODO remove if not used
        {
            foreach (UIElement child in panel.Children)
                child.Visibility = child == visible_child ? Visibility.Visible : Visibility.Collapsed;
        }

        public static IEnumerable<T> LogicalDescendants<T>(this DependencyObject dependency_object) where T : DependencyObject
            => LogicalDescendants(dependency_object).OfType<T>();

        private static IEnumerable<DependencyObject> LogicalDescendants(this DependencyObject dependancy_object)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(dependancy_object).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var child_of_child in LogicalDescendants(child))
                    yield return child_of_child;
            }
        }

        public static IEnumerable<T> VisualDescendants<T>(this DependencyObject dependency_object) where T : DependencyObject
            => VisualDescendants(dependency_object).OfType<T>();

        private static IEnumerable<DependencyObject> VisualDescendants(this DependencyObject dependency_object)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependency_object); i++)
            {
                var child = VisualTreeHelper.GetChild(dependency_object, i);
                yield return child;
                foreach (var child_of_child in VisualDescendants(child))
                    yield return child_of_child;
            }
        }

        public static bool IsDigit(this Key key, out int? digit)
        {
            digit = key switch
            {
                Key.D0 => 0,
                Key.D1 => 1,
                Key.D2 => 2,
                Key.D3 => 3,
                Key.D4 => 4,
                Key.D5 => 5,
                Key.D6 => 6,
                Key.D7 => 7,
                Key.D8 => 8,
                Key.D9 => 9,
                Key.NumPad0 => 0,
                Key.NumPad1 => 1,
                Key.NumPad2 => 2,
                Key.NumPad3 => 3,
                Key.NumPad4 => 4,
                Key.NumPad5 => 5,
                Key.NumPad6 => 6,
                Key.NumPad7 => 7,
                Key.NumPad8 => 8,
                Key.NumPad9 => 9,
                _ => null
            };
            return digit != null;
        }

        public static void SetSelectedItem(this TreeView tree_view, object item)
        {
            if (GetTreeViewItem(tree_view, item) is TreeViewItem tree_view_item)
                tree_view_item.IsSelected = true;
        }

        public static void ClearSelectedItem(this TreeView tree_view)
        {
            if (GetTreeViewItem(tree_view, tree_view.SelectedItem) is TreeViewItem tree_view_item)
                tree_view_item.IsSelected = false;
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// NOTE: Has the side-effect of collapsing all TreeViewItems except the path to the result. 
        /// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private static TreeViewItem? GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the
                // virtualizing case even if the item is marked
                // expanded we still need to do this step in order to
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter? itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter,
                    // so walk the descendents and find the child.
                    itemsPresenter = VisualDescendants<ItemsPresenter>(container).FirstOrDefault();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = VisualDescendants<ItemsPresenter>(container).FirstOrDefault();
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
                _ = itemsHostPanel.Children;

                var virtualizingPanel = itemsHostPanel; //MyVirtualizingStackPanel virtualizingPanel = itemsHostPanel as MyVirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(); //virtualizingPanel.BringIntoView(i);

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem? resultContainer = GetTreeViewItem(subContainer, item);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        //else
                        //{
                        //    // The object is not under this TreeViewItem
                        //    // so collapse it.
                        //    subContainer.IsExpanded = false;
                        //}
                    }
                }
            }

            return null;
        }
    }

    public static class StringExtenions
    {
        public static string ToProperCase(this string words)
        {
            if (string.IsNullOrEmpty(words))
                return "";
            words = string.Join(" ", words.Split(" ").Select(w => w.Capitalise()));
            words = string.Join("-", words.Split("-").Select(w => w.Capitalise()));
            return words;
        }

        public static string Capitalise(this string word)
        {
            if (string.IsNullOrEmpty(word))
                return "";
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }

        public static string UnCapitalise(this string word)
        {
            if (string.IsNullOrEmpty(word))
                return "";
            return word.Substring(0, 1).ToLower() + word.Substring(1);
        }

        public static string ToOrdinal(this int number)
        {
            if (number <= 0)
                throw new ArgumentException($"{nameof(number)} must be positive.");
            if (number == 11 || number == 12 || number == 13)
                return $"{number}th";
            else if (number % 10 == 1)
                return $"{number}st";
            else if (number % 10 == 2)
                return $"{number}nd";
            else if (number % 10 == 3)
                return $"{number}rd";
            else
                return $"{number}th";
        }

        public static string JoinWithCommas(this IEnumerable<string> things, bool use_and_instead_of_last_comma = true)
        {
            var e = things.GetEnumerator();
            if (!e.MoveNext())
                return "";
            var result = e.Current;
            bool moved_next = e.MoveNext();
            while (moved_next)
            {
                string thing = e.Current;
                moved_next = e.MoveNext();
                string sep = (moved_next || !use_and_instead_of_last_comma) ? ", " : " and ";
                result += sep + thing;
            }
            return result;
        }
    }
}

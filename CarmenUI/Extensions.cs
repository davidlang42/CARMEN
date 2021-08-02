using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarmenUI
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }
    }

    internal static class WpfExtensions
    {
        /// <summary>Collapses all children of the panel, except the given child, which is made visible.</summary>
        public static void ShowOneChild(this Panel panel, UIElement visible_child)
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
    }

    internal static class AsyncExtensions
    {
        public static Task<int> CountAsync<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            => Task.Run(() => collection.Count(predicate));

        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> collection)
            => Task.Run(() => collection.ToList());
    }
}

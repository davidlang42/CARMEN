using Microsoft.EntityFrameworkCore;
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

    internal static class EntityExtensions
    {
        /// <summary>There appears to be a significant delay the first time a DbSet<typeparamref name="T"/>
        /// property is accessed on the DbContext, therefore this extension method has been added to
        /// perform the DbSet__get and DbSet.Load() asyncronously. The result of the returned task
        /// is the DbSet itself, to assist in chaining.</summary>
        public static Task<DbSet<U>> ColdLoadAsync<T, U>(this T context, Func<T, DbSet<U>> db_set_getter) where T : DbContext where U : class
            => Task.Run(() =>
            {
                var db_set = db_set_getter(context);
                db_set.Load();
                return db_set;
            });

        /// <summary>There appears to be a significant delay the first time a DbSet<typeparamref name="T"/>
        /// property is accessed on the DbContext, therefore this extension method has been added to
        /// perform the DbSet__get and DbSet.Count() asyncronously.</summary>
        public static Task<int> ColdCountAsync<T, U>(this T context, Func<T, DbSet<U>> db_set_getter) where T : DbContext where U : class
            => Task.Run(() =>
            {
                var db_set = db_set_getter(context);
                return db_set.Count();
            });
    }
}

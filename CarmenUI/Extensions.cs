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
    }

    internal static class AsyncExtensions
    {
        public static Task<int> CountAsync<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            => Task.Run(() => collection.Count(predicate));

        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> collection)
            => Task.Run(() => collection.ToList());//LATER remove these if they aren't actually useful, or maybe should be delegated to casting engine async functions
    }

    internal static class WpfExtensions
    {
        /// <summary>Collapses all children of the panel, except the given child, which is made visible.</summary>
        public static void ShowOneChild(this Panel panel, UIElement visible_child)//LATER remove if not used
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
    }
}

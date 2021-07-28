using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

    }

    internal static class StringExtensions
    {
        static readonly char[] vowels = new[] { 'A', 'E', 'I', 'O', 'U' };
        static readonly char[] wordBoundaries = new[] { ' ', ',', '-', '\\', '/' };

        /// <summary>Abbreviates a phrase with the following heuristic:
        /// - if less than min_length words, concatenate capitalised words
        /// - otherwise, take the first letter of each word
        /// - if the word only contains digits, keep the whole number
        /// - reduce by removing lower case letters
        /// - reduce by removing vowels
        /// - reduce by removing letters from the middle</summary>
        public static string Abbreviate(this string full_text, int min_length = 2, int max_length = 6)
        {
            if (string.IsNullOrWhiteSpace(full_text))
                return "";
            var words = full_text.Split(wordBoundaries, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            char[] abbrev;
            if (words.Length < min_length)
            {
                abbrev = words.SelectMany(word => word.Capitalise().ToCharArray()).ToArray();
            }
            else
            {
                abbrev = words.SelectMany(word => {
                    if (word.All(ch => char.IsDigit(ch)))
                        return word;
                    else
                        return word.First().Yield();
                }).ToArray();
            }
            if (abbrev.Length > max_length)
            {
                var new_abbrev = abbrev.Where(ch => !char.IsLower(ch)).ToArray();
                if (new_abbrev.Length >= min_length)
                    abbrev = new_abbrev;
            }
            if (abbrev.Length > max_length)
            {
                var new_abbrev = abbrev.Where(ch => !vowels.Contains(ch)).ToArray();
                if (new_abbrev.Length >= min_length)
                    abbrev = new_abbrev;
            }
            while (abbrev.Length > max_length)
            {
                var middle = abbrev.Length / 2;
                var new_abbrev = new char[abbrev.Length - 1];
                int j = 0;
                for (var i = 0; i < abbrev.Length; i++)
                    if (i != middle)
                        new_abbrev[j++] = abbrev[i];
                abbrev = new_abbrev;
            }
            return string.Join("", abbrev);
        }

        public static string Capitalise(this string word)
        {
            if (string.IsNullOrEmpty(word))
                return "";
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }
    }
}

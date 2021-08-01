using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ShowModel
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Cast<int>().Sum();

        public static double Product(this IEnumerable<double> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("Cannot calculate product of an empty sequence.");
            var result = e.Current;
            while (e.MoveNext())
                result *= e.Current;
            return result;
        }

        /// <summary>Similar to SingleOrDefault(), except if the sequence contains more than 1 element,
        /// the default is returned rather than throwing an exception.</summary>
        public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                return default;
            var first = e.Current;
            if (e.MoveNext())
                return default;
            return first;
        }

        public static T NextOf<T>(this Random random, IList<T> list)
            => list[random.Next(list.Count)];
    }

    internal static class EntityExtensions
    {
        /// <summary>Use the property name as the column name, even if that means the column is shared with another property.
        /// This is useful when 2 inherited classes of the same base have a common property.</summary>
        public static void CommonProperty<T>(this EntityTypeBuilder<T> entity, string property) where T : class
            => entity.Property(property).HasColumnName(property);

        /// <summary>Configure the foreign key back to the owner of the Owned entity, and also configure the key
        /// of the Owned entity as a composite key { owner_key, another_field }.</summary>
        public static void WithOwnerCompositeKey<T, U>(this OwnedNavigationBuilder<T, U> builder, string owner_key, string composite_with_owner_key)
            where T : class
            where U : class
        {
            builder.WithOwner().HasForeignKey(owner_key);
            builder.HasKey(owner_key, composite_with_owner_key);
        }
    }

    public static class StringExtensions
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

        public static string Plural(this uint number, string suffix_single, string? suffix_plural = null)
        {
            suffix_plural ??= suffix_single + "s";
            return number == 1 ? $"{number} {suffix_single}" : $"{number} {suffix_plural}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.ShowModel
{
    public static class IEnumerableExtensions //TODO reuse extension methods using ShowModel as common ground
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Select(u => (int)u).Sum();

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> range)
        {
            foreach (var item in range)
                set.Add(item);
        }

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

        /// <summary>Faster than OfType<<typeparamref name="T"/>>()</summary>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> sequence) where T : class
            => sequence.Where(i => i != null).Cast<T>();

        /// <summary>Faster than OfType<<typeparamref name="T"/>>()</summary>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> sequence) where T : struct
            => sequence.Where(i => i != null).Cast<T>();
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
            => Plural((int)number, suffix_single, suffix_plural);

        public static string Plural(this int number, string suffix_single, string? suffix_plural = null)
        {
            suffix_plural ??= suffix_single + "s";
            return number == 1 ? $"{number} {suffix_single}" : $"{number} {suffix_plural}";
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

        public static string Initial(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            return name.Substring(0, 1);
        }

        public static string Plural(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            if (name.EndsWith("s"))
                return name + "'";
            return name + "s";
        }
    }
}

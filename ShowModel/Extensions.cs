using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Select(u => (int)u).Sum();

        /// <summary>Returns true if any values are added</summary>
        public static bool AddRange<T>(this HashSet<T> set, IEnumerable<T> collection)
        {
            bool added = false;
            foreach (var item in collection)
                added |= set.Add(item);
            return added;
        }

        /// <summary>Returns true if any values are removed</summary>
        public static bool RemoveRange<T>(this HashSet<T> set, IEnumerable<T> collection)
        {
            bool removed = false;
            foreach (var item in collection)
                removed |= set.Remove(item);
            return removed;
        }

        /// <summary>Returns true if any values are removed</summary>
        public static bool RemoveRange<T, U>(this Dictionary<T, U> dict, IEnumerable<T> collection)
            where T : notnull
        {
            bool removed = false;
            foreach (var item in collection)
                removed |= dict.Remove(item);
            return removed;
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

        /// <summary>Finds and removes the first item on the stack which matches the given predicate.
        /// NOTE: Only call this if you are sure you need to, because it requires popping and re-pushing every item
        /// prior to the one it finds.</summary>
        public static T? FindAndRemove<T>(this Stack<T> stack, Predicate<T> predicate)
            where T : class
        {
            var skipped = new Stack<T>();
            T? found = null;
            while (stack.Count > 0)
            {
                var next = stack.Pop();
                if (predicate(next))
                {
                    found = next;
                    break;
                }
                skipped.Push(next);
            }
            while (skipped.Count > 0)
                stack.Push(skipped.Pop());
            return found;
        }

        /// <summary>Removes all instances of the given object from the queue, returning the number of instances removed.
        /// NOTE: Only call this if you are sure you need to, because it requires re-writing the entire queue.</summary>
        public static int Remove<T>(this Queue<T> queue, T obj)
            where T : class
        {
            var original_count = queue.Count;
            for (var i = 0; i < original_count; i++)
            {
                var next = queue.Dequeue();
                if (next != obj)
                    queue.Enqueue(next);
            }
            return original_count - queue.Count;
        }

        public static T[][] Split<T>(this IEnumerable<T> sequence, int[] output_array_lengths)
        {
            var e = sequence.GetEnumerator();
            var results = new T[output_array_lengths.Length][];
            for (var r = 0; r < output_array_lengths.Length; r++)
            {
                results[r] = new T[output_array_lengths[r]];
                for (var i = 0; i < output_array_lengths[r]; i++)
                {
                    if (!e.MoveNext())
                        throw new ArgumentException($"Sequence was shorter than the sum of {nameof(output_array_lengths)}");
                    results[r][i] = e.Current;
                }
            }
            if (e.MoveNext())
                throw new ArgumentException($"Sequence was longer than the sum of {nameof(output_array_lengths)}");
            return results;
        }

        /// <summary>Calculates the min/max/mean/middle of a set of values
        /// NOTE: median will only be correct only if values are in order</summary>
        public static double Mean(this IEnumerable<uint> values, out uint min, out uint max, out double median_if_in_order)
        {
            var list = new List<uint>();
            uint total;
            var e = values.GetEnumerator();
            if (!e.MoveNext())
                return median_if_in_order = min = max = 0; // empty sequence
            list.Add(e.Current);
            total = min = max = e.Current;
            while (e.MoveNext())
            {
                list.Add(e.Current);
                total += e.Current;
                if (e.Current < min)
                    min = e.Current;
                if (e.Current > max)
                    max = e.Current;
            }
            var middle = list.Count / 2;
            if (list.Count % 2 == 0)
                median_if_in_order = (list[middle] + list[middle - 1]) / 2.0;
            else
                median_if_in_order = list[middle];
            return (double)total / list.Count;
        }
    }

    public static class AsyncExtensions
    {
        public static Task<int> CountAsync<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
            => Task.Run(() => collection.Count(predicate));

        public static Task<Dictionary<U, V>> ToDictionaryAsync<T, U, V>(this IEnumerable<T> collection, Func<T, U> key_selector, Func<T, V> value_selector) where U : notnull
            => Task.Run(() => collection.ToDictionary(key_selector, value_selector));
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

        readonly static char[] wordSeparators = new[] { ' ', '-', '\'' };
        public static string Initial(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            var sb = new StringBuilder();
            char? last_separator = ' ';
            foreach (var ch in name)
            {
                if (wordSeparators.Contains(ch))
                {
                    if (last_separator == ' ')
                        last_separator = ch;
                    else
                        last_separator ??= ch;
                }
                else if (last_separator.HasValue)
                {
                    if (last_separator.Value != ' ')
                        sb.Append(last_separator.Value);
                    sb.Append(char.ToUpper(ch));
                    last_separator = null;
                }
            }
            return sb.ToString();
        }

        public static string Plural(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            if (name.EndsWith("s"))
                return name + "'";
            return name + "s";
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
}

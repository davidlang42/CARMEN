using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A sort method which works on an imperfect comparison function, that is one where A > B > C does not always mean A > C.
    /// This sorter implements IComparer<typeparamref name="T"/> using the cached results of calling the provided imperfect comparer.
    /// </summary>
    public class DisagreementSort<T> : IComparer<T> //LATER remove if not used
        where T : class
    {
        IComparer<T> imperfectComparer;
        Dictionary<(T, T), int> comparisonCache = new();

        public DisagreementSort(IComparer<T> imperfect_comparer)
        {
            imperfectComparer = imperfect_comparer;
        }

        public IEnumerable<T> Sort(IEnumerable<T> items)
        {
            var sorted = new List<HashSet<T>>();
            var e = items.GetEnumerator();
            if (!e.MoveNext())
                yield break;
            // First item is always sorted
            sorted.Add(new HashSet<T> { e.Current });
            // Insert each item in order (grouping as required)
            while (e.MoveNext())
            {
                // Find the range which the new item sit in
                int greater_than; // e.Current is greater than everything before (not including) this index
                for (greater_than = 0; greater_than < sorted.Count; greater_than++)
                    if (!GreaterThanAll(e.Current, sorted[greater_than]))
                        break;
                int less_than; // e.Current is less than everything after (not including) this index
                for (less_than = sorted.Count - 1; less_than >= 0; less_than--)
                    if (!LessThanAll(e.Current, sorted[less_than]))
                        break;
                // Insert the new item
                if (greater_than - less_than == 1)
                    // The new item has a proper position, insert it there
                    sorted.Insert(greater_than, new HashSet<T> { e.Current });
                else if (greater_than <= less_than)
                    // Group everything which might be equal to the new item in one slot
                    GroupTogether(sorted, greater_than, less_than, e.Current);
                else
                    // Something wacky is going on if this gets hit regularly
                    GroupTogether(sorted, less_than, greater_than, e.Current);
            }
            // If only one resulting group, tie breaking is required (to stop infinite recursion)
            if (sorted.SingleOrDefaultSafe() is HashSet<T> only_group && only_group.Count > 1)
            {
                var item_with_the_most_other_items_greater_than_it = only_group
                    .OrderByDescending(item => only_group
                        .Count(other_item => other_item != item && GreaterThan(other_item, item)))
                    .First();
                yield return item_with_the_most_other_items_greater_than_it;
                only_group.Remove(item_with_the_most_other_items_greater_than_it);
            }
            // Enumerate in order, sorting each group recursively
            foreach (var set in sorted)
            {
                if (set.Count == 1)
                    yield return set.First();
                else
                    foreach (var item in Sort(set))
                        yield return item;
            }
        }

        /// <summary>Combine the sets at indicies of <paramref name="list"/> between <paramref name="start_index"/> and
        /// <paramref name="end_index"/> (inclusive) into one set, and add the <paramref name="new_item"/>.</summary>
        private void GroupTogether(List<HashSet<T>> list, int start_index, int end_index, T new_item)
        {
            // Group together everything which is tied with the new item
            var combined = list[start_index];
            for (var i = end_index; i > start_index; i--)
            {
                combined.AddRange(list[i]);
                list.RemoveAt(i);
            }
            // Split the group if possible
            var greater_than_items = combined.Where(item => GreaterThan(new_item, item)).ToHashSet();
            var less_than_items = combined.Where(item => LessThan(new_item, item)).ToHashSet();
            if (greater_than_items.All(gti => less_than_items.All(lti => GreaterThan(gti, lti))))
            {
                list.Insert(start_index + 1, greater_than_items); // after combined
                list.Insert(start_index, less_than_items); // before combined
                combined.RemoveRange(greater_than_items);
                combined.RemoveRange(less_than_items);
            }
            // Add the new item
            combined.Add(new_item);
        }

        private bool GreaterThanAll(T item, IEnumerable<T> greater_than_items)
        {
            foreach (var greater_than_item in greater_than_items)
                if (!GreaterThan(item, greater_than_item))
                    return false;
            return true;
        }

        private bool LessThanAll(T item, IEnumerable<T> less_than_items)
        {
            foreach (var less_than_item in less_than_items)
                if (!LessThan(item, less_than_item))
                    return false;
            return true;
        }

        private bool GreaterThan(T item, T greater_than_item)
            => Compare(item, greater_than_item) > 0;

        private bool LessThan(T item, T less_than_item)
            => Compare(item, less_than_item) < 0;

        public int Compare(T? a, T? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            if (comparisonCache.TryGetValue((a, b), out var cached_result))
                return cached_result;
            var result = imperfectComparer.Compare(a, b);
            comparisonCache.Add((a, b), result);
            return result;
        }
    }
}

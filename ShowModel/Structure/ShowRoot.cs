using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ShowModel.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode //LATER implement INotifyPropertyChanged for completeness
    {
        public struct ConsecutiveItemResult
        {
            public Item Item1;
            public Item Item2;
            public int CastCount;
        }
        public DateTime? ShowDate { get; set; }
        public virtual Image? Logo { get; set; }
        /// <summary>The criteria which will be used to determine the order for Cast Numbers (if not set, OverallAbility will be used)</summary>
        public virtual Criteria? CastNumberOrderBy { get; set; }
        public ListSortDirection CastNumberOrderDirection { get; set; }
        public override InnerNode? Parent
        {
            get => null;
            set
            {
                if (value != null)
                    throw new InvalidOperationException("Parent of ShowRoot must be null.");
            }
        }

        public bool VerifyConsecutiveItems(out List<ConsecutiveItemResult> failures)
            => VerifyConsecutiveItems(out failures, false);

        public bool VerifyConsecutiveItems()
            => VerifyConsecutiveItems(out _, true);

        private bool VerifyConsecutiveItems(out List<ConsecutiveItemResult> failures, bool shortcut_result)
        {
            failures = new();
            var items = ItemsInOrder().ToList();
            var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList(); //LATER does this need await? also parallelise
            for (var i = 1; i < items.Count; i++)
            {
                var cast_in_consecutive_items = cast_per_item[i].Intersect(cast_per_item[i - 1]).Count(); //LATER does this need await? also confirm that in-built Intersect() isn't slower than hashset.select(i => other_hashset.contains(i)).count()
                if (cast_in_consecutive_items != 0)
                {
                    failures.Add(new ConsecutiveItemResult { Item1 = items[i - 1], Item2 = items[i], CastCount = cast_in_consecutive_items });
                    if (shortcut_result)
                        return false;
                }
            }
            return failures.Count == 0;
        }
    }
}

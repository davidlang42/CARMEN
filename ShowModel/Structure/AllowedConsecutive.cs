using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// A set of consecutive items where applicants are allowed to be cast in both
    /// </summary>
    public class AllowedConsecutive
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int AllowedConsecutiveId { get; private set; }

        private readonly ObservableCollection<Item> items = new();
        /// <summary>The consecutive items in which applicants can be consecutively cast.
        /// Must contain at least 2 items.</summary>
        public virtual ICollection<Item> Items => items;

        private readonly ObservableCollection<Applicant> cast = new();
        /// <summary>A whitelist of applicants which are allowed to be consecutively cast between the given items.
        /// If empty, any applicants may be consecutively cast between the given items.</summary>
        public virtual ICollection<Applicant> Cast => cast;

        public bool IsAllowed(Applicant applicant)
            => Cast.Count == 0 || Cast.Contains(applicant);

        /// <summary>Checks that this allowed consecutive contains at least 2 items, and that they are consecutive items</summary>
        public bool IsValid(ShowRoot show_root)
        {
            if (Items.Count < 2)
                return false;
            var e = show_root.ItemsInOrder().GetEnumerator();
            var remaining = Items.ToHashSet();
            // find the first item in this set
            while (e.MoveNext())
                if (remaining.Remove(e.Current))
                    break;
            // remove all that are consecutive
            while (e.MoveNext())
                if (!remaining.Remove(e.Current))
                    break;
            // valid if nothing remaining (ie. all items were consecutive)
            return remaining.Count == 0;
        }
    }
}

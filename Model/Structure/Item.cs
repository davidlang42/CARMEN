using Model.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Model.Structure
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : Node
    {
        #region Database fields
        public override string Name { get; set; } = "Item";
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();
        #endregion

        public override IEnumerable<Item> ItemsInOrder() => this.Yield();

        public override uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).SingleOrDefault()?.Count
            ?? Roles.Select(r => r.CountFor(group)).Sum();

        public Item? NextItem()
        {
            var e = RootParent().ItemsInOrder().GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current == this)
                    return e.MoveNext() ? e.Current : null;
            }
            throw new ApplicationException("Item not found in root parent's items.");
        }

        public Item? PreviousItem()
        {
            var e = RootParent().ItemsInOrder().GetEnumerator();
            Item? last_item = null;
            while (e.MoveNext())
            {
                if (e.Current == this)
                    return last_item;
                last_item = e.Current;
            }
            throw new ApplicationException("Item not found in root parent's items.");
        }
    }
}

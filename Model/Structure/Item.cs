using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Structure
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : Node, ICounted
    {
        #region Database fields
        public override string Name { get; set; } = "Item";
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public override IEnumerable<Item> ItemsInOrder() => this.Yield();

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroupId == group.CastGroupId).SingleOrDefault()?.Count
            ?? Roles.Select(r => r.CountFor(group)).Sum();

        public Item? NextItem() => throw new NotImplementedException(); //TODO implement NextItem
        public Item? PreviousItem() => throw new NotImplementedException(); //TODO implement PreviousItem
    }
}

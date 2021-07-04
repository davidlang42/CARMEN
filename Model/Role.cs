using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Model
{
    public class Role
    {
        #region Database fields
        [Key]
        public int RoleId { get; private set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        //TODO public virtual ICollection<Requirement> Requirements { get; private set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroupId == group.CastGroupId).Select(c => c.Count).SingleOrDefault(); // defaults to 0

        public uint TotalCount()
            => Convert.ToUInt32(CountByGroups.Sum(c => c.Count)); // will crash if greater than UInt32.MaxValue
    }
}

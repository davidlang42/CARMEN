using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using Model.Structure;
using Model.Requirements;

namespace Model
{
    public class Role : ICounted
    {
        #region Database fields
        [Key]
        public int RoleId { get; private set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => c.Count).SingleOrDefault(); // defaults to 0
    }
}

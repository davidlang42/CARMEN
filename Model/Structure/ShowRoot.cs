using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode, ICounted
    {
        #region Database fields
        public override string Name { get; set; } = "Show";
        public DateTime? ShowDate { get; set; }
        public virtual Image? Logo { get; set; }
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        public override InnerNode? Parent
        {
            get => null;
            set
            {
                if (value != null)
                    throw new InvalidOperationException("Parent of ShowRoot must be null.");
            }
        }
        #endregion

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroupId == group.CastGroupId).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();
    }
}

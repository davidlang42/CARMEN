using Model.Applicants;
using System;
using System.Linq;

namespace Model.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode
    {
        #region Database fields
        public override string Name { get; set; } = "Show";
        public DateTime? ShowDate { get; set; }
        public virtual Image? Logo { get; set; }
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

        public override uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();
    }
}

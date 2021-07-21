using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ShowModel.Applicants;

namespace ShowModel.Structure
{
    /// <summary>
    /// A node in the item tree, which may or may not be able to have children.
    /// </summary>
    public abstract class Node : IOrdered, ICounted, IValidatable
    {
        #region Database fields
        [Key]
        public int NodeId { get; private set; }
        public abstract string Name { get; set; }
        public int Order { get; set; }
        public virtual InnerNode? Parent { get; set; }
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public abstract IEnumerable<Item> ItemsInOrder();

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();

        public Node RootParent() => Parent?.RootParent() ?? this;

        /// <summary>Checks if this node's required counts equals the sum of the roles within it.</summary>
        public virtual IEnumerable<string> Validate()
        {
            foreach (var cbg in CountByGroups)
            {
                var actual = ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(cbg.CastGroup)).Sum();
                if (cbg.Count != actual)
                    yield return $"Actual roles for {cbg.CastGroup.Name} ({actual}) does not equal required count ({cbg.Count}).";
            }
        }
    }

    /// <summary>
    /// An internal node of the item tree, which can have children.
    /// </summary>
    public abstract class InnerNode : Node
    {
        public virtual ICollection<Node> Children { get; private set; } = new ObservableCollection<Node>();

        public override IEnumerable<Item> ItemsInOrder() => Children.InOrder().SelectMany(n => n.ItemsInOrder());
    }
}

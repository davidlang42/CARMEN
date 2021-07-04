using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Model
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : Node
    {
        #region Database fields
        /// <summary>A list of nodes (eg. Section or Item) which are children to this node.</summary>
        public virtual ICollection<Node> Children { get; private set; } = new ObservableCollection<Node>();
        #endregion

        internal override IEnumerable<Item> ItemsInOrder()
            => Children.InOrder().SelectMany(n => n.ItemsInOrder());
    }
}

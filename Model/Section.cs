using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : Node
    {
        #region Database fields
        public virtual SectionType SectionType { get; set; } = null!;
        public virtual ICollection<Node> Children { get; private set; } = new ObservableCollection<Node>();
        #endregion

        public override IEnumerable<Item> ItemsInOrder() => Children.InOrder().SelectMany(n => n.ItemsInOrder());
    }
}

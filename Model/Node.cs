using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Model
{
    /// <summary>
    /// A node in the item tree (eg. Section or Item).
    /// </summary>
    public abstract class Node : IOrdered
    {
        #region Database fields
        [Key]
        public int NodeId { get; private set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public virtual Node? Parent { get; set; }
        public virtual ICollection<Node> Children { get; private set; } = new ObservableCollection<Node>();
        #endregion

        public virtual IEnumerable<Item> ItemsInOrder() => Children.InOrder().SelectMany(n => n.ItemsInOrder());
    }
}

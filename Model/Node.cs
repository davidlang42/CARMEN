using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    /// <summary>
    /// A node in the item tree.
    /// Eg. Section or Item
    /// </summary>
    public abstract class Node : IOrdered
    {
        #region Database fields
        [Key]
        public int NodeId { get; private set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public Show Show { get; set; } = null!;
        /// <summary>The parent section (or null if root) of this node.</summary>
        public Section? Parent { get; set; }
        #endregion

        internal abstract IEnumerable<Item> ItemsInOrder();
    }
}

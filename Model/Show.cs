using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Model
{
    /// <summary>
    /// The root object, a show, which contains applicants, items, criteria and everything else required to cast a show.
    /// </summary>
    public class Show
    {
        #region Database fields
        public int ShowId { get; private set; }
        public virtual ICollection<Applicant> Applicants { get; private set; } = new ObservableCollection<Applicant>();
        public virtual ICollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public string Name { get; set; } = "";
        public DateTime? ShowDate { get; set; }
        #endregion

        public IEnumerable<Item> ItemsInOrder()
            => Nodes.InOrder().SelectMany(n => n.ItemsInOrder());
    }
}

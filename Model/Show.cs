using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Model
{
    /// <summary>
    /// The root object, a show, which contains applicants, items, criteria and everything else required to cast a show.
    /// </summary>
    public class Show
    {
        public int ShowId { get; private set; }
        public virtual ICollection<Applicant> Applicants { get; private set; } = new ObservableCollection<Applicant>();
        public virtual ICollection<Section> Sections { get; private set; } = new ObservableCollection<Section>();
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();//TODO probably dont need both sections and items
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public string Name { get; set; } = "";
        public DateTime? ShowDate { get; set; }
    }
}

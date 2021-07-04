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
    public class Show : Node
    {
        #region Database fields
        public virtual ICollection<Applicant> Applicants { get; private set; } = new ObservableCollection<Applicant>();
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public DateTime? ShowDate { get; set; }
        #endregion

        public override Node? Parent
        {
            get => null;
            set => throw new InvalidOperationException("Show cannot have a parent.");
        }
    }
}

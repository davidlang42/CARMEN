using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ShowModel.Applicants;
using ShowModel.Requirements;

namespace ShowModel.Structure
{
    public class Role : ICounted, INamed //LATER implement INotifyPropertyChanged for completeness
    {
        public enum RoleStatus
        {
            NotCast,
            FullyCast,
            UnderCast,
            OverCast
        }

        [Key]
        public int RoleId { get; private set; } //LATER make all IDs internal rather than public (this would require patching DatabaseExplorer to not show/use IDs)
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        public virtual ICollection<Applicant> Cast { get; private set; } = new ObservableCollection<Applicant>();

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => c.Count).SingleOrDefault(); // defaults to 0

        public RoleStatus Status
        {
            get
            {
                return RoleStatus.FullyCast;//TODO
            }
        }
    }
}

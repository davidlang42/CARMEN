using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A group of people which an applicant can be selected into.
    /// An applicant can have many Tags.
    /// </summary>
    public class Tag : INameOrdered, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int TagId { get; private set; }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
            }
        }

        private string description = "";
        public string Description
        {
            get => description;
            set
            {
                if (description == value)
                    return;
                description = value;
                OnPropertyChanged();
            }
        }

        private Image? icon;
        public virtual Image? Icon
        {
            get => icon;
            set
            {
                if (icon == value)
                    return;
                icon = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Applicant> members = new();
        public virtual ICollection<Applicant> Members => members;

        private ObservableCollection<Requirement> requirements = new();
        public virtual ICollection<Requirement> Requirements => requirements;

        private ObservableCollection<CountByGroup> countByGroups = new();
        public virtual ICollection<CountByGroup> CountByGroups => countByGroups;

        public uint? CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => (uint?)c.Count).SingleOrDefault();

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

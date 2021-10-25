using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Carmen.ShowModel.Requirements;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A group of people which an applicant can be selected into.
    /// Cast groups are mutually exclusive.
    /// </summary>
    public class CastGroup : IOrdered, INamed, INotifyPropertyChanged, IValidatable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int CastGroupId { get; private set; }

        private int order;
        public int Order
        {
            get => order;
            set
            {
                if (order == value)
                    return;
                order = value;
                OnPropertyChanged();
            }
        }

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
                if (name != "")
                    Abbreviation = name.Abbreviate(2, 4);
            }
        }

        private string abbreviation = "";
        public string Abbreviation
        {
            get => abbreviation;
            set
            {
                if (abbreviation == value)
                    return;
                abbreviation = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Applicant> members = new();
        public virtual ICollection<Applicant> Members => members;

        private uint? requiredCount;
        /// <summary>The number of applicants which should be allocated to this group (per alternate cast)</summary>
        public uint? RequiredCount
        {
            get => requiredCount;
            set
            {
                if (requiredCount == value)
                    return;
                requiredCount = value;
                OnPropertyChanged();
            }
        }

        private bool alternateCasts;
        public bool AlternateCasts
        {
            get => alternateCasts;
            set
            {
                if (alternateCasts == value)
                    return;
                alternateCasts = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Requirement> requirements = new();
        public virtual ICollection<Requirement> Requirements => requirements;

        public CastGroup()
        {
            members.CollectionChanged += Members_CollectionChanged; //TODO dispose handlers
            requirements.CollectionChanged += Requirements_CollectionChanged;
        }

        /// <summary>Gets the number of FTE members of this cast group, taking AlternateCasts into account</summary>
        public uint FullTimeEquivalentMembers(int alternative_cast_count) //TODO is this actually needed/correct anywhere?
            => (uint)(AlternateCasts ? Members.Count / alternative_cast_count : Members.Count);

        private void Members_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Members));

        private void Requirements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Requirements));

        protected void OnPropertyChanged([CallerMemberName]string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public IEnumerable<string> Validate()
        {
            foreach (var tag_req in Requirements.SelectMany(r => r.References()).OfType<TagRequirement>())
                yield return $"Cast group '{Name}' cannot have tag requirement '{tag_req.Name}' because cast groups are selected before tags are applied.";
        }
    }
}

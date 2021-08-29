using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A person who has auditioned to be in a show.
    /// </summary>
    public class Applicant : IValidatable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Registration properties
        [Key]
        public int ApplicantId { get; private set; }

        private string firstName = "";
        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName == value)
                    return;
                firstName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRegistered));
            }
        }

        private string lastName = "";
        public string LastName
        {
            get => lastName;
            set
            {
                if (lastName == value)
                    return;
                lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRegistered));
            }
        }

        private Gender? gender;
        public Gender? Gender
        {
            get => gender;
            set
            {
                if (gender == value)
                    return;
                gender = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(IsRegistered));
            }
        }

        private DateTime? dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => dateOfBirth;
            set
            {
                if (dateOfBirth == value)
                    return;
                dateOfBirth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeToday));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(IsRegistered));
            }
        }

        private string externalData = "";
        public string ExternalData
        {
            get => externalData;
            set
            {
                if (externalData == value)
                    return;
                externalData = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Audition properties
        private Image? photo;
        public virtual Image? Photo
        {
            get => photo;
            set
            {
                if (photo == value)
                    return;
                photo = value;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<Ability> abilities = new();
        public virtual ICollection<Ability> Abilities => abilities;

        private string notes = "";
        public string Notes
        {
            get => notes;
            set
            {
                if (notes == value)
                    return;
                notes = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Selection & Casting properties
        private CastGroup? castGroup;
        public virtual CastGroup? CastGroup
        {
            get => castGroup;
            set
            {
                if (castGroup == value)
                    return;
                castGroup = value;
                OnPropertyChanged();
                if (castGroup == null)
                {
                    CastNumber = null;
                    AlternativeCast = null;
                }    
            }
        }

        private int? castNumber;
        public int? CastNumber
        {
            get => castNumber;
            set
            {
                if (castNumber == value)
                    return;
                castNumber = value;
                OnPropertyChanged();
            }
        }

        public string? CastNumberAndCast => CastNumber == null ? null : $"{CastNumber}{AlternativeCast?.Initial}";

        private AlternativeCast? alternativeCast;
        public virtual AlternativeCast? AlternativeCast
        {
            get => alternativeCast;
            set
            {
                if (alternativeCast == value)
                    return;
                alternativeCast = value;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<Tag> tags = new();
        public virtual ICollection<Tag> Tags => tags;

        private readonly ObservableCollection<Role> roles = new();
        public virtual ICollection<Role> Roles => roles;
        #endregion

        public uint? AgeToday => AgeAt(DateTime.Now);

        public string Description
        {
            get
            {
                var gender = Gender?.ToString();
                var age = AgeToday?.Plural("year old", "years old");
                return string.Join(", ", new[] { gender, age }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public bool IsRegistered
            => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) && Gender.HasValue && DateOfBirth.HasValue;

        public bool HasAuditioned(IEnumerable<Criteria> all_criterias)
            => HasAuditioned(IsRegistered, Abilities, all_criterias);

        public static bool HasAuditioned(bool is_registered, IEnumerable<Ability> applicant_abilities, IEnumerable<Criteria> all_criterias)
        {
            if (!is_registered)
                return false;
            foreach (var ability in applicant_abilities)
                if (ability.Mark > ability.Criteria.MaxMark)
                    return false;
            foreach (var required_criteria in all_criterias.Where(c => c.Required))
                if (!applicant_abilities.Any(ab => ab.Criteria == required_criteria))
                    return false;
            return true;
        }

        public Applicant()
        {
            abilities.CollectionChanged += Abilities_CollectionChanged;
            tags.CollectionChanged += Tags_CollectionChanged;
            roles.CollectionChanged += Roles_CollectionChanged;//LATER check any show model INotifyPropertyChanged objects for handlers that should be disposed
        }

        private void Abilities_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Abilities));
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                var new_items = e.NewItems?.Cast<Ability>().ToHashSet() ?? new HashSet<Ability>();
                var old_items = e.OldItems?.Cast<Ability>().ToHashSet() ?? new HashSet<Ability>();
                foreach (var added in new_items.Where(n => !old_items.Contains(n)))
                    added.PropertyChanged += Ability_PropertyChanged;
                foreach (var removed in old_items.Where(o => !new_items.Contains(o)))
                    removed.PropertyChanged -= Ability_PropertyChanged;
            }
        }

        private void Ability_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            => OnPropertyChanged(nameof(Abilities));

        private void Tags_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Tags));

        private void Roles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Roles));

        public uint? AgeAt(DateTime date) //LATER handle errors more nicely, move errors to validation
        {
            if (DateOfBirth == null)
                return null;
            if (DateOfBirth > date)
                throw new ArgumentOutOfRangeException($"Date of birth is after the provided {nameof(date)}.");
            return (uint)((date - DateOfBirth.Value).TotalDays / 365.2425); // correct on average, good enough
        }

        public uint MarkFor(Criteria criteria)
            => Abilities.Where(a => a.Criteria == criteria).SingleOrDefault()?.Mark ?? 0;

        public string FormattedMarkFor(Criteria criteria) => criteria.Format(MarkFor(criteria));

        /// <summary>Determines if an Applicant has been accepted into the cast.
        /// This is determined by membership in a CastGroup.</summary>
        public bool IsAccepted => CastGroup != null;

        /// <summary>Checks that names are not blank, gender and date of birth are set, and all criteria have marks within range.</summary>
        public IEnumerable<string> Validate()
        {
            if (string.IsNullOrEmpty(FirstName))
                yield return "First Name cannot be blank.";
            if (string.IsNullOrEmpty(LastName))
                yield return "Last Name cannot be blank.";
            if (!Gender.HasValue)
                yield return "Gender must be set.";
            if (!DateOfBirth.HasValue)
                yield return "Date of Birth must be set.";
            foreach (var ability in Abilities)
                if (ability.Mark > ability.Criteria.MaxMark)
                    yield return $"Mark for {ability.Criteria.Name} ({ability.Mark}) is greater than the maximum ({ability.Criteria.MaxMark}).";
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public enum Gender
    {
        Male,
        Female
    }
}

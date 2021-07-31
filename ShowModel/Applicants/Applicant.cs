using ShowModel.Criterias;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ShowModel.Applicants
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

        public int OverallAbility
            => Convert.ToInt32(Abilities.Sum(a => a.Mark / a.Criteria.MaxMark * a.Criteria.Weight)); //LATER handle overflow

        public Applicant()
        {
            abilities.CollectionChanged += Abilities_CollectionChanged;
            tags.CollectionChanged += Tags_CollectionChanged;
            roles.CollectionChanged += Roles_CollectionChanged;
        }

        private void Abilities_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

        /// <summary>Determines if an Applicant has been accepted into the cast.
        /// This is determined by membership in a CastGroup.</summary>
        public bool IsAccepted() => CastGroup != null;

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

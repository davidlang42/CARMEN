﻿using Carmen.ShowModel.Criterias;
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
    public class Applicant : INotifyPropertyChanged, IComparable<Applicant>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Registration properties
        [Key]
        public int ApplicantId { get; private set; }

        /// <summary>Applicant needs a reference to ShowRoot in order to know the ShowDate,
        /// so their age at show can be calculated</summary>
        public virtual ShowRoot ShowRoot { get; set; } = null!;

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
                var max_date = (ShowRoot?.ShowDate ?? DateTime.Now);
                if (value > max_date)
                    value = max_date;
                if (dateOfBirth == value)
                    return;
                dateOfBirth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Age));
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
        /// <summary>May be null if Photo has been set but not saved</summary>
        public int? PhotoImageId { get; private set; }

        private Image? photo;
        public virtual Image? Photo
        {
            get => photo;
            set
            {
                if (photo == value)
                    return;
                photo = value;
                PhotoImageId = null;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<Ability> abilities = new();
        public virtual ICollection<Ability> Abilities => abilities;

        private readonly ObservableCollection<Note> notes = new();
        public virtual ICollection<Note> Notes => notes;

        private readonly ObservableCollection<AllowedConsecutive> allowedConsecutives = new();
        public virtual ICollection<AllowedConsecutive> AllowedConsecutives => allowedConsecutives;
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
                if (castGroup is CastGroup existing)
                    existing.Members.Remove(this);
                castGroup = value;
                OnPropertyChanged();
                AlternativeCast = null;
                if (castGroup == null)
                {
                    CastNumber = null;
                    foreach (var role in Roles)
                        role.Cast.Remove(this);
                    Roles.Clear();
                    foreach (var tag in Tags)
                        tag.Members.Remove(this);
                    Tags.Clear();
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
                if (alternativeCast is AlternativeCast existing)
                    existing.Members.Remove(this);
                alternativeCast = value;
                OnPropertyChanged();
                CastNumber = null;
            }
        }

        private SameCastSet? sameCastSet;
        public virtual SameCastSet? SameCastSet
        {
            get => sameCastSet;
            set
            {
                if (sameCastSet == value)
                    return;
                if (sameCastSet is SameCastSet existing)
                    existing.Applicants.Remove(this);
                sameCastSet = value;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<Tag> tags = new();
        public virtual ICollection<Tag> Tags => tags;

        private readonly ObservableCollection<Role> roles = new();
        public virtual ICollection<Role> Roles => roles;
        #endregion

        /// <summary>Calculates the applicant's age at the show date, if set, otherwise age as of today</summary>
        public uint? Age => ShowRoot.ShowDate.HasValue ? AgeAt(ShowRoot.ShowDate.Value) : AgeAt(DateTime.Now);

        public string? Description
        {
            get
            {
                var gender = Gender?.ToString();
                var age = Age?.Plural("year old", "years old");
                if (gender == null && age == null)
                    return null;
                return string.Join(", ", new[] { gender, age }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public bool IsRegistered
            => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) && Gender.HasValue && Age.HasValue;

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
                if (!applicant_abilities.Any(ab => ab.Criteria.CriteriaId == required_criteria.CriteriaId))
                    return false;
            return true;
        }

        public Applicant()
        {
            abilities.CollectionChanged += Abilities_CollectionChanged;
            tags.CollectionChanged += Tags_CollectionChanged;
            roles.CollectionChanged += Roles_CollectionChanged;
            notes.CollectionChanged += Notes_CollectionChanged;
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

        private void Notes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Notes));

        public uint? AgeAt(DateTime date)
        {
            if (DateOfBirth == null || DateOfBirth > date)
                return null; // missing dob, or dob in the future
            return (uint)((date - DateOfBirth.Value).TotalDays / 365.2425); // correct on average, good enough
        }

        public uint MarkFor(Criteria criteria)
            => Abilities.Where(a => a.Criteria == criteria).SingleOrDefault()?.Mark ?? 0;

        public void SetMarkFor(Criteria criteria, uint? mark)
        {
            if (Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is not Ability ability)
            {
                if (mark == null)
                    return; // nothing to do
                ability = new Ability
                {
                    Applicant = this,
                    Criteria = criteria
                };
                Abilities.Add(ability);
            }
            if (mark == null)
                Abilities.Remove(ability);
            else
                ability.Mark = mark.Value;
        }

        public string FormattedMarkFor(Criteria criteria) => criteria.Format(MarkFor(criteria));

        /// <summary>Determines if an Applicant has been accepted into the cast.
        /// This is determined by membership in a CastGroup.</summary>
        public bool IsAccepted => CastGroup != null;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>This really shouldn't be required, but after EntityEntry.Reload() is called,
        /// you need to manually trigger the PropertyChanged event.</summary>
        public void NotifyChanged() => OnPropertyChanged("");

        public int CompareTo(Applicant? other) => ApplicantId.CompareTo(other?.ApplicantId);
    }
}

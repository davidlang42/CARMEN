using Model.Criterias;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Model.Applicants
{
    /// <summary>
    /// A person who has auditioned to be in a show.
    /// </summary>
    public class Applicant
    {
        #region Database fields
        [Key]
        public int ApplicantId { get; private set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public virtual Image? Photo { get; set; }
        public string Notes { get; set; } = "";
        public virtual ICollection<Ability> Abilities { get; private set; } = new ObservableCollection<Ability>();
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public virtual ICollection<Identity> Identities { get; private set; } = new ObservableCollection<Identity>();
        #endregion

        public uint AgeToday() => AgeAt(DateTime.Now);

        public uint AgeAt(DateTime date)
        {
            if (DateOfBirth > date)
                throw new ArgumentOutOfRangeException($"{DisplayName()}'s DOB is after the provided {nameof(date)}.");
            return (uint)((date - DateOfBirth).TotalDays / 365.2425); // correct on average, good enough
        }

        public string DisplayName() => FirstName + " " + LastName;

        public int OverallAbility() => Convert.ToInt32(Abilities.Sum(a => a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

        public uint MarkFor(Criteria criteria)
            => Abilities.Where(a => a.Criteria == criteria).SingleOrDefault()?.Mark ?? 0;

        /// <summary>Determines if an Applicant has been accepted into the cast.
        /// This is determined by membership in a CastGroup with Primary set to true.</summary>
        public bool IsAccepted() => CastGroups.Any(g => g.Primary);
    }

    public enum Gender
    {
        Male,
        Female
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Model
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
        public string? ExternalID { get; set; }
        public virtual Image? Photo { get; set; }
        public string Notes { get; set; } = "";
        public virtual ICollection<Ability> Abilities { get; private set; } = new ObservableCollection<Ability>();
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        #endregion

        public uint AgeToday() => AgeAt(DateTime.Now);

        /// <summary>Applicant's age at the show date, or null if show date is not set.</summary>
        //TODO public uint? AgeAtShow() => Show.ShowDate.HasValue ? AgeAt(Show.ShowDate.Value) : (uint?)null;

        public uint AgeAt(DateTime date)
        {
            if (DateOfBirth > date)
                throw new ArgumentOutOfRangeException($"{DisplayName()}'s DOB is after the provided {nameof(date)}.");
            return (uint)((date - DateOfBirth).TotalDays / 365.2425); // correct on average, good enough
        }

        public string DisplayName() => FirstName + " " + LastName;

        public int OverallAbility() => Convert.ToInt32(Abilities.Sum(a => a.Mark / a.Criteria.MaxMark * a.Criteria.Weight)); // may crash if Criteria Weights sum to greater than Int32.MaxValue

        public uint GetMarkFor(Criteria criteria)
            => Abilities.Where(a => a.Criteria == criteria).SingleOrDefault()?.Mark ?? 0;
    }

    public enum Gender
    {
        Male,
        Female
    }
}

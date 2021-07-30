using ShowModel.Criterias;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShowModel.Applicants
{
    /// <summary>
    /// A person who has auditioned to be in a show.
    /// </summary>
    public class Applicant : IValidatable
    {
        #region Database fields (registering)
        [Key]
        public int ApplicantId { get; private set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ExternalData { get; set; } = "";
        #endregion

        #region Database fields (auditioning)
        public virtual Image? Photo { get; set; }
        public virtual ICollection<Ability> Abilities { get; private set; } = new ObservableCollection<Ability>();
        public string Notes { get; set; } = "";
        #endregion

        #region Database fields (selection & casting)
        public virtual CastGroup? CastGroup { get; set; }
        public int? CastNumber { get; set; }
        public virtual AlternativeCast? AlternativeCast { get; set; }
        public virtual ICollection<Tag> Tags { get; private set; } = new ObservableCollection<Tag>();
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();
        #endregion

        public uint AgeToday => AgeAt(DateTime.Now); //TODO dont do this here, because age wont update with bday, use a value converter

        public uint AgeAt(DateTime date) //LATER handle errors more nicely, probably return nullable, move errors to validation
        {
            if (DateOfBirth == null)
                throw new ApplicationException($"Date of birth is not set.");
            if (DateOfBirth > date)
                throw new ArgumentOutOfRangeException($"Date of birth is after the provided {nameof(date)}.");
            return (uint)((date - DateOfBirth.Value).TotalDays / 365.2425); // correct on average, good enough
        }

        public int OverallAbility() => Convert.ToInt32(Abilities.Sum(a => a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

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
    }

    public enum Gender
    {
        Male,
        Female
    }
}

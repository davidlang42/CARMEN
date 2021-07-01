using System;
using System.Collections.Generic;

namespace Model
{
    public class Applicant
    {
        public Guid ApplicantId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ExternalID { get; set; }
        //TODO photo
        //TODO calculate weighted overall mark
        //TODO public virtual List<Ability> Abilities { get; set; }
        public int Age => (int)((DateTime.Now - DateOfBirth).TotalDays / 365.2425); // correct on average, good enough
    }

    public enum Gender
    {
        Male,
        Female
    }
}

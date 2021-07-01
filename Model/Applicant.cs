using System;
using System.Collections.Generic;

namespace Model
{
    public class Applicant
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ExternalID { get; set; }
        //TODO photo
        //TODO calculate weighted overall mark
        //TODO public virtual List<Ability> Abilities { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }
}

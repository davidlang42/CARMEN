using System;

namespace Carmen.ShowModel.Applicants
{
    public enum Gender
    {
        Male,
        Female
    }

    public static class GenderExtensions
    {
        public static char ToChar(this Gender gender) 
            => gender switch
            {
                Gender.Male => 'M',
                Gender.Female => 'F',
                _ => throw new NotImplementedException($"Gender not handled: {gender}")
            };
}
}

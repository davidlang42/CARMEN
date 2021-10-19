using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;

namespace Carmen.CastingEngine.Allocation
{
    public class ApplicantForRoleComparer : IComparer<Applicant>
    {
        Role role;
        IComparer<(Applicant, Role)> comparer;

        public ApplicantForRoleComparer(IComparer<(Applicant, Role)> comparer, Role for_role)
        {
            this.comparer = comparer;
            role = for_role;
        }

        public int Compare(Applicant? x, Applicant? y)
        {
            if (x == null || y is null)
                throw new ArgumentNullException();
            return comparer.Compare((x, role), (y, role));
        }
    }
}

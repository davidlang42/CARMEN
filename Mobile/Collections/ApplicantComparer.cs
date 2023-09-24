using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Collections
{
    internal class ApplicantComparer : IComparer<Applicant>
    {
        public List<Func<Applicant, IComparable?>> ByFields { get; set; } = new();

        public int Compare(Applicant? x, Applicant? y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1; // order null first
            if (y == null)
                return 1;
            foreach (var getter in ByFields)
            {
                var x_field = getter(x);
                var y_field = getter(y);
                if (x_field == null && y_field == null)
                    continue;
                if (x_field == null)
                    return -1; // order null first
                if (y_field == null)
                    return 1;
                var result = x_field.CompareTo(y_field);
                if (result != 0)
                    return result;
            }
            return 0; // if all ByFields match, then they are considered equal
        }

        public readonly static ApplicantComparer NameLastFirst = new()
        {
            ByFields =
            {
                a => a.LastName,
                a => a.FirstName
            }
        };

        public readonly static ApplicantComparer NameFirstLast = new()
        {
            ByFields =
            {
                a => a.FirstName,
                a => a.LastName
            }
        };
    }
}

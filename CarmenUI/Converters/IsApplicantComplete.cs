using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Checks if a provided applicant object is complete. ConverterParameter must be a collection of all criterias.
    /// </summary>
    public class IsApplicantComplete : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ArgumentException("ConverterParameter must be an CollectionViewSource containing all criterias.");
            if (view_source.Source is not IList list)
                return false;
            if (value is not Applicant applicant)
                return false;
            if (string.IsNullOrEmpty(applicant.FirstName))
                return false;
            if (string.IsNullOrEmpty(applicant.LastName))
                return false;
            if (!applicant.Gender.HasValue)
                return false;
            if (!applicant.DateOfBirth.HasValue)
                return false;
            foreach (var ability in applicant.Abilities)
                if (ability.Mark > ability.Criteria.MaxMark)
                    return false;
            foreach (var required_criteria in list.OfType<Criteria>().Where(c => c.Primary))
                if (!applicant.Abilities.Any(ab => ab.Criteria == required_criteria))
                    return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

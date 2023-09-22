using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// Checks if a provided applicant has completed their audition.
    /// Values provided must be {IsRegistered, Abilities}.
    /// ConverterParameter must be a collection of all criterias.
    /// Return value is Visibility.Visible if true, otherwise Visibility.Collapsed.
    /// </summary>
    public class ShowIfApplicantRegisteredAndAuditioned : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {IsRegistered, Abilities}");
            if (values[0] is not bool is_registered)
                return Visibility.Collapsed;
            if (values[1] is not IEnumerable<Ability> abilities)
                return Visibility.Collapsed;
            if (parameter is not CollectionViewSource view_source)
                throw new ArgumentException("ConverterParameter must be an CollectionViewSource containing all criterias.");
            if (view_source.Source is not IList list)
                return Visibility.Collapsed;
            return Applicant.HasAuditioned(is_registered, abilities, list.OfType<Criteria>()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

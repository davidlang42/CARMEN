using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Converts an Ability into a formatted Mark
    /// </summary>
    class AbilityMarkFormatter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Ability ability)
                return null;
            return ability.Criteria.Format(ability.Mark);
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

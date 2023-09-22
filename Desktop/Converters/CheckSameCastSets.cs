using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// Convert {AlternativeCast, SameCastSet} into a highlight color if the SameCastSet of applicants are not all in the same cast.
    /// </summary>
    class CheckSameCastSets : IMultiValueConverter
    {
        BrushConverter brushConverter = new();

        public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {AlternativeCast, SameCastSet}");
            if (values[0] is not AlternativeCast alternative_cast || values[1] is not SameCastSet set)
                return null;
            if (set.Applicants.All(a => a.AlternativeCast == null || a.AlternativeCast == alternative_cast))
                return null;
            return brushConverter.ConvertFromString(HighlightError.ERROR_COLOR);
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

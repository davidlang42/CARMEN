using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// Converts a single-value or multi-value input into a boolean of whether or not all input values match the parameter
    /// </summary>
    internal class EqualityConverter : IMultiValueConverter, IValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return true;
            var first = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] != first)
                    return false;
            return Convert(first, targetType, parameter, culture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == parameter;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

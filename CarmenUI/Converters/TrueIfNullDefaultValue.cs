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
    /// Convert an object value to a Boolean based on whether or not it is null.
    /// Similar to TrueIfNull, but converts both ways and the parameter is the value to be set when true.
    /// </summary>
    public class TrueIfNullDefaultValue : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
            => value == null;

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? null : (parameter is string s ? FixType(s, targetType) : parameter);

        private static object FixType(string value, Type target)
        {
            if (target == typeof(double?))
                return double.Parse(value);
            return value;
        }
    }
}

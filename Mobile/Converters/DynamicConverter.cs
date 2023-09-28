using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Carmen.Mobile.Converters
{
    /// <summary>
    /// A converter which takes 2 values {Value, Converter} and converts the given value with the given converter.
    /// This allows binding the choice of converter to something dynamic.
    /// </summary>
    internal class DynamicConverter : IMultiValueConverter
    {
        public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[1] is not IValueConverter converter)
                throw new ArgumentException("Expected exactly 2 values: Value, Converter");
            return converter.Convert(values[0], targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

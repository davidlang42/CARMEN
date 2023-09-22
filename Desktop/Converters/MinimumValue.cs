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
    /// A converter which returns true if the T value is greater than or equal to the T ConverterParameter
    /// </summary>
    public class MinimumValue<T> : IValueConverter where T : IComparable
    {
        public delegate bool ParseFunction(string? str, out T value);

        ParseFunction parse;

        public MinimumValue(ParseFunction parse)
        {
            this.parse = parse;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!parse(parameter?.ToString(), out T minimum_value))
                throw new ApplicationException("Parameter not set to a valid value.");
            if (value == null || value is not T typed_value && !parse(value?.ToString(), out typed_value))
                return null;
            return typed_value.CompareTo(minimum_value) >= 0;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class MinimumIntValue : MinimumValue<int>
    {
        public MinimumIntValue() : base(int.TryParse)
        { }
    }
}

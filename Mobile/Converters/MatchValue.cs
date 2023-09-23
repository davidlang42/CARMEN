using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Converters
{
    /// <summary>
    /// Compare the value to a fixed value and return true if it matches.
    /// When converting back, true will return the fixed value, false will return the default value for type T.
    /// </summary>
    public class MatchValue<T> : IValueConverter where T : struct//TODO copied from Carmen.Desktop.Converters
    {
        public delegate bool ParseFunction(string? str, out T value);

        ParseFunction parse;

        public MatchValue(ParseFunction parse)
        {
            this.parse = parse;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!parse(parameter?.ToString(), out T match_value))
                throw new ApplicationException("Parameter not set to a valid value.");
            if (value == null)
                return null;
            return value.Equals(match_value);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!parse(parameter?.ToString(), out T match_value))
                throw new ApplicationException("Parameter not set to a valid value.");
            if (value == null)
                return null;
            if (value is bool b_value && b_value)
                return match_value;
            return default(T);
        }
    }

    public class MatchUIntValue : MatchValue<uint>
    {
        public MatchUIntValue() : base(uint.TryParse)
        { }
    }
}

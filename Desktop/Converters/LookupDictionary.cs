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
    /// A converter which looks up the value in a Dictionary, set as a property of the converter object.
    /// The value is the key used to lookup (as a string), and the ConverterParameter provides a default return value.
    /// This is a one-way coverter only.
    /// </summary>
    public class LookupDictionary<T> : IValueConverter
    {
        public Dictionary<string, T> Dictionary { get; set; } = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() is string key && Dictionary.TryGetValue(key, out var lookup_value))
                return lookup_value;
            return parameter ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BooleanLookupDictionary : LookupDictionary<bool>
    { }
}

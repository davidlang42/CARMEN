using System;
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
    /// Convert an object value to a Visibility based on whether or not it is null.
    /// Similar to HideIfNull, except that if ConverterParameter is set to a type, then the value must NOT be this type in order to be shown.
    /// </summary>
    public class HideIfNullOrType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Hidden;
            if (parameter is Type type && (value.GetType() == type || value.GetType().IsSubclassOf(type)))
                return Visibility.Hidden;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

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
    /// A multi-value converter which returns the first value which is not null
    /// </summary>
    public class FallbackValues : IMultiValueConverter
    {
        public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
                if (value != null)
                    return value;
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

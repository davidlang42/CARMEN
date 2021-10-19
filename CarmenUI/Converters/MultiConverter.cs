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
    /// Converts the value multiple times through a list of converters.
    /// NOTE: ConverterParameter will be passed to all converters, and targetType is essentially meaningless.
    /// </summary>
    public class MultiConverter : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var converter in this)
                value = converter.Convert(value, targetType, parameter, culture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var converter in this.AsEnumerable().Reverse())
                value = converter.ConvertBack(value, targetType, parameter, culture);
            return value;
        }
    }
}

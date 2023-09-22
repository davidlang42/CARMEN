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
    /// A value converter which converts true to 1 and false to 0, two-ways, useful for combo boxes
    /// </summary>
    class BooleanToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? 1 : 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i && i == 1;
    }
}

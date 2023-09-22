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
    /// A value converter which two-way maps 0d to false and any other double to true.
    /// The value which it should be set to for true is provided in the parameter, defaulting to 1d.
    /// </summary>
    class TrueIfNonZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double d && d != 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? parameter is string s ? double.Parse(s) : parameter as double? ?? 1d : 0d;
    }
}

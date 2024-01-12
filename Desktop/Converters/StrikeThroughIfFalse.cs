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
    /// Converts a boolean into null if true, or TextDecorations.Strikethrough if false
    /// </summary>
    internal class StrikeThroughIfFalse : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return null;
            else
                return TextDecorations.Strikethrough;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

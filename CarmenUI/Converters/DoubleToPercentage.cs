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
    /// Format a double value as a percentage (string).
    /// If provided, the ConverterParameter is the int number of decimal places, otherwise 0.
    /// </summary>
    public class DoubleToPercentage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!int.TryParse(parameter?.ToString(), out int precision))
                precision = 0;
            if (value is double number)
                return $"{Math.Round(number * 100, precision)}%";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

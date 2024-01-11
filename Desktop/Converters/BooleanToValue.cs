using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    internal class BooleanToValue : IValueConverter
    {
        object valueIfTrue, valueIfFalse;

        public BooleanToValue(object value_if_true, object value_if_false)
        {
            valueIfTrue = value_if_true;
            valueIfFalse = value_if_false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? valueIfTrue : valueIfFalse;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

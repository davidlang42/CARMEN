using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// A multi value converter which takes multiple values, but passes through the first value only
    /// to a series of other converters. The other input values can be used as a way to trick the
    /// Binding into updating when they change.
    /// NOTE: ConverterParameter will be passed to all converters, and targetType is essentially meaningless.
    /// </summary>
    public class FakeItTilYouUpdateIt : List<IValueConverter>, IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values.FirstOrDefault();
            foreach (var converter in this)
                value = converter.Convert(value, targetType, parameter, culture);
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

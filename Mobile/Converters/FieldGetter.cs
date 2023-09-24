using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Converters
{
    internal class FieldGetter<T> : IValueConverter
    {
        readonly Func<T, object> getter;

        public FieldGetter(Func<T, object> getter)
        {
            this.getter = getter;
        }

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not T typed)
                return null;
            return getter(typed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

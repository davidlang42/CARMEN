using ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Takes multiple objects of type INamed, and returns a concatenated string list of Names.
    /// </summary>
    public class NameLister : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable list)
                return string.Join(", ", list.OfType<INamed>().Select(n => n.Name));
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

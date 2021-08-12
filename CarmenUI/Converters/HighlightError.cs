using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CarmenUI.Converters
{
    /// <summary>
    /// A converter which takes a boolean and if true, returns a red highlight color string, otherwise transparent.
    /// </summary>
    public class HighlightError : IValueConverter
    {
        BrushConverter brushConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => brushConverter.ConvertFromString((value is bool b && b) ? "LightCoral" : "Transparent"); //LATER put these constants somewhere

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

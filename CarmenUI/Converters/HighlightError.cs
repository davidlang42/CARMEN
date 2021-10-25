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
        public const string ERROR_COLOR = "LightCoral"; // also referenced in AllocateRoles.xaml, SelectCast.xaml, ConfigureItems.xaml
        public const string TEXTBOX_COLOR = "White";
        public const string PANEL_COLOR = "WhiteSmoke";
        public const string TRANSPARENT_COLOR = "Transparent";

        BrushConverter brushConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => brushConverter.ConvertFromString((value is bool b && b) ? ERROR_COLOR : TRANSPARENT_COLOR);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

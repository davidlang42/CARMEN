using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Converters
{
    /// <summary>
    /// Convert {Author, Timestamp} into a formatted description string.
    /// </summary>
    public class NoteDescription : IMultiValueConverter
    {
        public object Convert(object?[] values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {Author, Timestamp}");
            var author = values[0] as string;
            var timestamp = values[1] as DateTime?;
            if (author == null || timestamp == null)
                return "";
            return $"{author} ({timestamp})";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

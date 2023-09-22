using Carmen.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    public class ProcessStatusColor : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessStatus status)
                value = status switch
                {
                    ProcessStatus.None => "LightYellow",
                    ProcessStatus.Loading => "#15000000",
                    ProcessStatus.Complete => "#2200FF00",
                    ProcessStatus.Error => "#1AFF0000",
                    _ => throw new NotImplementedException($"Enum not handled: {status}")
                };
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

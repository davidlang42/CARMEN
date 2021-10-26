using CarmenUI.ViewModels;
using FontAwesome.WPF;
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
    public class ProcessStatusIcon : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessStatus status)
                value = status switch
                {
                    ProcessStatus.None => null,
                    ProcessStatus.Loading => new ImageAwesome
                    {
                        Icon = FontAwesomeIcon.Spinner,
                        Spin = true,
                        SpinDuration = 1.5,
                        Foreground = Brushes.DimGray
                    },
                    ProcessStatus.Complete => new ImageAwesome
                    {
                        Icon = FontAwesomeIcon.Check,
                        Foreground = Brushes.Green
                    },
                    ProcessStatus.Error => new ImageAwesome
                    {
                        Icon = FontAwesomeIcon.Times,
                        Foreground = Brushes.Red
                    },
                    _ => throw new NotImplementedException($"Enum not handled: {status}")
                };
            return value;
        }
        
        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

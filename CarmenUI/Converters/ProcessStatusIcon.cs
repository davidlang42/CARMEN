using CarmenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    public class ProcessStatusIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessStatus status)
                value = status switch
                {
                    ProcessStatus.None => "",
                    ProcessStatus.Loading => "/Icons/Ellipsis_bold.png",
                    ProcessStatus.Complete => "/Icons/Checkmark.png",
                    ProcessStatus.Error => "/Icons/Cancel.png",
                    _ => throw new NotImplementedException($"Enum not handled: {status}")
                };
            return value;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

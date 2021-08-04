using ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    public class CountByGroupsTotal : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IList list)
                throw new ArgumentException("Value must be an IList containing CountByGroups.");
            return list.OfType<CountByGroup>().Select(cbg => cbg.Count).Sum();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

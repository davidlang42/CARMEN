using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    internal class RoleFullyCast : IValueConverter
    {
        AlternativeCast[] alternativeCasts;

        public RoleFullyCast(AlternativeCast[] alternative_casts)
        {
            this.alternativeCasts = alternative_casts;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Role role)
                return false;
            return role.CastingStatus(alternativeCasts) == Role.RoleStatus.FullyCast;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

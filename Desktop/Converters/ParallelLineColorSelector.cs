using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// Converts {Role, RoleCastCount(fake), [ApplicantForRole.IsSelected..]} to a brush based on the results.
    /// </summary>
    internal class ParallelLineColorSelector : IMultiValueConverter
    {
        AlternativeCast[] alternativeCasts;
        Color colorIfApplicantDoubleCast, colorIfRoleFullyCast, defaultColor;

        public ParallelLineColorSelector(AlternativeCast[] alternative_casts, Color colorIfApplicantDoubleCast, Color colorIfRoleFullyCast, Color defaultColor)
        {
            this.alternativeCasts = alternative_casts;
            this.colorIfApplicantDoubleCast = colorIfApplicantDoubleCast;
            this.colorIfRoleFullyCast = colorIfRoleFullyCast;
            this.defaultColor = defaultColor;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var selected_roles_count = values.Skip(2).Where(v => v is bool b && b).Count();
            if (selected_roles_count > 1)
                return new SolidColorBrush { Color = colorIfApplicantDoubleCast };
            // ignore values[1]
            if (values[0] is Role role && role.CastingStatus(alternativeCasts) == Role.RoleStatus.FullyCast)
                return new SolidColorBrush { Color = colorIfRoleFullyCast };
            return new SolidColorBrush { Color = defaultColor };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

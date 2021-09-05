using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
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
    /// <summary>
    /// The hackiest of hacks to identify nodes with any errors on ConfigureItems UI.
    /// The whole Node must be passed in as the value, with a Dictionary&lt;CastGroup,uint&gt;
    /// (representing the FTE cast members per group) in the ConverterParameter.
    /// </summary>
    public class AnyNodeErrors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source
                || view_source.Source is not Dictionary<CastGroup, uint> cast_members)
                return true; // incorrect parameter type
            if (value is not Node node)
                return true; // incorrect value type
            if (node is Item item)
            {
                if (item.Roles.Count == 0)
                    return true;
                if (item.Roles.Any(r => string.IsNullOrEmpty(r.Name) || r.CountByGroups.Sum(cbg => cbg.Count) == 0))
                    return true;
            }
            if (node is InnerNode inner && inner.Children.Count == 0)
                return true;
            if (!node.CountMatchesSumOfRoles())
                return true;
            if (node is Section section && section.RolesMatchCastMembers(cast_members) != Section.RolesMatchResult.RolesMatch)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

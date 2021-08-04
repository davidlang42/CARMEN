using ShowModel.Applicants;
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
    /// <summary>
    /// Selects a count from a list of CountByGroups, for the set CastGroup.
    /// </summary>
    public class CountByGroupSelector : IValueConverter
    {
        public CastGroup CastGroup { get; set; }

        public CountByGroupSelector(CastGroup cast_group)
        {
            CastGroup = cast_group;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IList list)
                throw new ArgumentException("Value must be an IList containing CountByGroups.");
            return list.OfType<CountByGroup>().Where(cbg => cbg.CastGroup == CastGroup).SingleOrDefault()?.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();//TODO implement two-way
    }
}

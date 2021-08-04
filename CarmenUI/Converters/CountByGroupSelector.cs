using CarmenUI.ViewModels;
using ShowModel.Applicants;
using ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// This converter wraps an ObservableCollection&lt;CountByGroup&gt; as a single NullableCountByGroup,
    /// for a specified CastGroup, allowing a non-set CountByGroup to be modelled as null.
    /// ConverterParameter must be the CastGroup for which this is wrapping.
    /// </summary>
    public class CountByGroupSelector : IValueConverter//TODO remove if not required
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CastGroup cast_group)
                throw new ApplicationException("ConverterParameter must be a CastGroup.");
            if (value is ObservableCollection<CountByGroup> collection)
                return new NullableCountByGroup(collection, cast_group);
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

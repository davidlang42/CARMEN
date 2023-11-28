using Carmen.Desktop.ViewModels;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using Carmen.ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// This converter wraps an ObservableCollection&lt;CountByGroup&gt; as a List&lt;NullableCountByGroup&gt;,
    /// allowing a non-set CountByGroups to be modelled as null.
    /// ConverterParameter must be a CollectionViewSource representing the CastGroups.
    /// </summary>
    public class SparseCountByGroups : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the CastGroups.");
            if (value is ObservableCollection<CountByGroup> collection && view_source.Source is IList list)
                return list.OfType<CastGroup>().InOrder().Select(g => new NullableCountByGroup(collection, g)).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

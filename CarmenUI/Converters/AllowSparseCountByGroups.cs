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
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// This converter wraps an ObservableCollection&lt;CountByGroup&gt; as a List&lt;NullableCountByGroup&gt;,
    /// allowing a non-set CountByGroups to be modelled as null.
    /// </summary>
    public class AllowSparseCountByGroups : IValueConverter
    {
        public CollectionViewSource? CastGroups;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (CastGroups == null)
                throw new ApplicationException("Tried to wrap CountByGroups without setting CastGroups.");
            if (value is ObservableCollection<CountByGroup> collection && CastGroups.Source is IList list)
                return list.OfType<CastGroup>().Select(g => new NullableCountByGroup(collection, g)).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

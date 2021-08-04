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
    /// This converter passes through an ObservableCollection&lt;CountByGroup&gt;, adding adds any missing CastGroups to the collection.
    /// ConverterParameter must be a CollectionViewSource representing the CastGroups.
    /// </summary>
    public class FullyEnumeratedCountByGroups : IValueConverter //LATER remove if not used
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the CastGroups.");
            if (value is ObservableCollection<CountByGroup> collection && view_source.Source is IList list)
                foreach (var cast_group in list.OfType<CastGroup>())
                    if (!collection.Any(cbg => cbg.CastGroup == cast_group))
                        collection.Add(new CountByGroup { CastGroup = cast_group });
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

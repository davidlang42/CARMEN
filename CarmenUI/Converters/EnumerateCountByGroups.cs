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
    /// </summary>
    public class EnumerateCountByGroups : IValueConverter
    {
        public CollectionViewSource? CastGroups;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (CastGroups == null)
                throw new ApplicationException("Tried to enumerate CountByGroups without setting CastGroups.");
            if (value is ObservableCollection<CountByGroup> collection && CastGroups.Source is IList list)
                foreach (var cast_group in list.OfType<CastGroup>()) // filter out non-CastGroup objects, as the source may contain "Loading..."
                    if (!collection.Any(cbg => cbg.CastGroup == cast_group))
                        collection.Add(new CountByGroup { CastGroup = cast_group });
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

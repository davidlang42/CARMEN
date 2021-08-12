using CarmenUI.ViewModels;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Carmen.ShowModel.Structure;

namespace CarmenUI.Converters
{
    /// <summary>
    /// This converter wraps an ObservableCollection&lt;T&gt; as a List&lt;SelectableObject<T>&gt;.
    /// ConverterParameter must be a CollectionViewSource representing the full list of T objects.
    /// </summary>
    public class SelectableObjectList<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the Criterias.");
            if (value is ObservableCollection<T> collection && view_source.Source is IList list)
                return list.OfType<T>().Select(c => new SelectableObject<T>(collection, c)).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class SelectableRequirementsList : SelectableObjectList<Requirement>
    { }

    public class SelectableItemsList : SelectableObjectList<Item>
    { }
}

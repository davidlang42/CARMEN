using Carmen.Desktop.ViewModels;
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

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// This converter wraps an ObservableCollection&lt;T&gt; as a List&lt;SelectableObject&lt;T&gt;&gt;.
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

    /// <summary>
    /// A special case of SelectableObjectList&lt;T&gt; which wraps a Role's ObservableCollection&lt;Item&gt; as a List&lt;SelectableObject&lt;Item&gt;&gt;.
    /// The value must be the whole Role object, and ConverterParameter must be a CollectionViewSource representing the full list of T objects.
    /// </summary>
    public class SelectableItemsList : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the Criterias.");
            if (value is Role role && view_source.Source is IList list)
                return list.OfType<Item>().Select(item => new SelectableObject<Item>(
                    role.Items, item, // keeps Role.Items up to date
                    i => i.Roles.Add(role), i => i.Roles.Remove(role) // keeps Item.Roles up to date
                )).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

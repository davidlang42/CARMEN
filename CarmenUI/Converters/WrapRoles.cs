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
    /// This converter wraps an ObservableCollection&lt;Role&gt; as a List&lt;RoleView&gt;,
    /// allowing a non-set CountByGroups to be modelled as null, and accessed via an array.
    /// ConverterParameter must be a CollectionViewSource representing the CastGroups.
    /// </summary>
    public class WrapRoles : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the CastGroups.");
            if (value is ObservableCollection<Role> roles && view_source.Source is IList list)
                return roles.Select(r => new RoleView(r, list.OfType<CastGroup>().ToArray())).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

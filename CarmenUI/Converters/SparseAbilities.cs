using CarmenUI.ViewModels;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
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
    /// This converter wraps an ObservableCollection&lt;Ability&gt; as a List&lt;NullableAbility&gt;,
    /// allowing a non-set Ability to be modelled as null.
    /// ConverterParameter must be a CollectionViewSource representing the Criterias.
    /// </summary>
    public class SparseAbilities : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not CollectionViewSource view_source)
                throw new ApplicationException("ConverterParameter must be a CollectionViewSource representing the Criterias.");
            if (value is ObservableCollection<Ability> collection && view_source.Source is IList list)
                return list.OfType<Criteria>().Select(c => new NullableAbility(collection, c)).ToList();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

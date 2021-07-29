using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DatabaseExplorer
{
    public class SortIOrdered : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list) {
                var view = new ListCollectionView(list);
                view.SortDescriptions.Add(new SortDescription(nameof(ShowModel.IOrdered.Order), ListSortDirection.Ascending));
                value = view;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

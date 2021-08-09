using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DatabaseExplorer
{
    public class SortIOrdered : IValueConverter
    {
        public static SortDescription SortDescription = new(nameof(Carmen.ShowModel.IOrdered.Order), ListSortDirection.Ascending);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list)
                value = new ListCollectionView(list) { SortDescriptions = { SortDescription } };
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

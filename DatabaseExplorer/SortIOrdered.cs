using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DatabaseExplorer
{
    public class SortIOrdered : IValueConverter
    {
        // Source: https://stackoverflow.com/questions/5722835/how-to-sort-treeview-items-using-sortdescriptions-in-xaml
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)value;
            var view = new ListCollectionView(collection);
            SortDescription sort = new SortDescription(nameof(Model.IOrdered.Order), ListSortDirection.Ascending);
            view.SortDescriptions.Add(sort);
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

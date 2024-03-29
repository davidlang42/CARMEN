﻿using Carmen.ShowModel;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    public class SortIOrdered : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list)
                return new ListCollectionView(list) { SortDescriptions = { StandardSort.For<IOrdered>() }, IsLiveSorting = true };
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

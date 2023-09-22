﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Converters
{
    /// <summary>
    /// Convert {FirstName, LastName} into a formatted name string.
    /// </summary>
    public class FullName : IMultiValueConverter
    {
        public object Convert(object?[] values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {FirstName, LastName}");
            var first = values[0] as string;
            var last = values[1] as string;
            if (first == null || last == null)
                return "";
            return $"{first} {last}"; //TODO implement as a user setting, similar to (or the same as) Carmen.Desktop.Converters.FullName
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

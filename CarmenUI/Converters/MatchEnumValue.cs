using CarmenUI.ViewModels;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Compare the value of an enum to a fixed value and return true if it matches.
    /// Helpful for binding enum values to ratio buttons.
    /// </summary>
    public class MatchEnumValue<T> : IValueConverter where T : struct
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.TryParse<T>(parameter?.ToString(), out var match_value))
                throw new ApplicationException("Parameter not set to a valid enum value.");
            if (value is T enum_value)
                return enum_value.Equals(match_value);
            return DependencyProperty.UnsetValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.TryParse<T>(parameter?.ToString(), out var match_value))
                throw new ApplicationException("Parameter not set to a valid enum value.");
            if (value is bool b_value && b_value)
                return match_value;
            return DependencyProperty.UnsetValue;
        }
    }

    public class MatchGenderValue : MatchEnumValue<Gender>
    { }

    public class MatchStatusValue : MatchEnumValue<ProcessStatus>
    { }
}
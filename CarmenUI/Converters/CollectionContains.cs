using System;
using System.Collections;
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
    /// A converter which checks if a set value exists within the collection provided as the ConverterParameter, returning boolean.
    /// In reverse, if a boolean is provided, then the set value will be added/removed from the ConverterParameter collection, returning the collection.
    /// </summary>
    public class CollectionContains : IValueConverter
    {
        public object? RequiredValue { get; set; }

        public CollectionContains(object? required_value = null)
        {
            RequiredValue = required_value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is IList list)
                return list.Contains(RequiredValue);
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b_value && parameter is IList list)
                if (b_value)
                {
                    if (!list.Contains(parameter))
                        list.Add(parameter);
                }
                else
                {
                    if (list.Contains(parameter))
                        list.Remove(parameter);
                }
            return parameter;
        }
    }
}

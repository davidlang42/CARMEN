using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Convert {int count, CastGroup/AlternativeCast/Tag} into a highlight color if supplied count does not match required count.
    /// </summary>
    class CheckRequiredCount : IMultiValueConverter
    {
        public int AlternativeCasts { get; set; }
        public int AlternatingCastGroups { get; set; }

        BrushConverter brushConverter = new();

        public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {int count, CastGroup/AlternativeCast/Tag}");
            if (values.Any(v => v == DependencyProperty.UnsetValue))
                return null;
            if (values[0] is not int count)
                throw new ApplicationException("Count must be int");
            var obj = values[1];
            bool error;
            if (obj is CastGroup cg)
                error = cg.RequiredCount is uint required ? (cg.AlternateCasts ? required * AlternativeCasts : required) != count
                    : count % AlternativeCasts == 0; // when no required count, make sure count is divisible by AlternativeCasts
            else if (obj is AlternativeCast ac)
            {
                var remaining = AlternatingCastGroups;
                error = ac.Members.Where(a => a.CastGroup != null)
                    .GroupBy(a => a.CastGroup)
                    .Wrap(_ => remaining--)
                    .Any(g => g.Count() != g.Key!.Members.Count / AlternativeCasts)
                    || remaining != 0;
            }
            else if (obj is Tag t)
                error = t.Members.Where(a => a.CastGroup != null)
                    .GroupBy(a => (a.CastGroup, a.AlternativeCast))
                    .Any(g => t.CountFor(g.Key.CastGroup!) is uint required && required != g.Count());
            else
                error = false;
            return brushConverter.ConvertFromString(error ? HighlightError.ERROR_COLOR : HighlightError.TRANSPARENT_COLOR);
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

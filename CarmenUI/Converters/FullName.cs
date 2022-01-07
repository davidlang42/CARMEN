using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Convert {FirstName, LastName} into a formatted name string.
    /// The format of the name can be set by the ConverterParameter, but defaults to the user setting.
    /// </summary>
    public class FullName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {FirstName, LastName}");
            var first = values[0] as string;
            var last = values[1] as string;
            if (first == null || last == null)
                return "";
            return Format(first, last, parameter as FullNameFormat?);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();


        public static string Format(Applicant applicant, FullNameFormat? format = null)
            => Format(applicant.FirstName, applicant.LastName, format);

        public static string Format(string first, string last, FullNameFormat? format = null)
        {
            format ??= Properties.Settings.Default.FullNameFormat;
            return format switch
            {
                FullNameFormat.FirstLast => $"{first} {last}",
                FullNameFormat.LastCommaFirst => $"{last}, {first}",
                FullNameFormat.InitialDotLast => $"{first.Initial()}. {last}",
                FullNameFormat.LastCommaInitialDot => $"{last}, {first.Initial()}.",
                FullNameFormat.FirstInitialDot => $"{first} {last.Initial()}.",
                FullNameFormat.InitialDotCommaFirst => $"{last.Initial()}., {first}",
                FullNameFormat.Initials => $"{first.Initial()}{last.Initial()}",
                FullNameFormat.InitialsDots => $"{first.Initial()}.{last.Initial()}.",
                _ => throw new NotImplementedException($"Enum not handled: {format}")
            };
        }
    }

    public enum FullNameFormat
    {
        /// <summary>David Lang</summary>
        FirstLast,
        /// <summary>Lang, David</summary>
        LastCommaFirst,
        /// <summary>D. Lang</summary>
        InitialDotLast,
        /// <summary>Lang, D.</summary>
        LastCommaInitialDot,
        /// <summary>David L.</summary>
        FirstInitialDot,
        /// <summary>L., David</summary>
        InitialDotCommaFirst,
        /// <summary>DL</summary>
        Initials,
        /// <summary>D.L.</summary>
        InitialsDots,
    }

    public static class FullNameFormatExtensions
    {
        public static IEnumerable<SortDescription> ToSortDescriptions(this FullNameFormat format, ListSortDirection direction = ListSortDirection.Ascending)
        {
            switch (format)
            {
                case FullNameFormat.FirstLast:
                case FullNameFormat.InitialDotLast:
                case FullNameFormat.FirstInitialDot:
                case FullNameFormat.Initials:
                case FullNameFormat.InitialsDots:
                    return new SortDescription[] { new(nameof(Applicant.FirstName), direction), new(nameof(Applicant.LastName), direction) };
                case FullNameFormat.LastCommaFirst:
                case FullNameFormat.LastCommaInitialDot:
                case FullNameFormat.InitialDotCommaFirst:
                    return new SortDescription[] { new(nameof(Applicant.LastName), direction), new(nameof(Applicant.FirstName), direction) };
                default:
                    throw new NotImplementedException($"Enum not handled: {format}");
            }
        }
    }
}

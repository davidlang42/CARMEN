using ShowModel.Applicants;
using System;
using System.Collections.Generic;
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
                FullNameFormat.InitialDotLast => $"{Initial(first)}. {last}",
                FullNameFormat.LastCommaInitialDot => $"{last}, {Initial(first)}.",
                FullNameFormat.FirstInitialDot => $"{first} {Initial(last)}.",
                FullNameFormat.InitialDotCommaFirst => $"{Initial(last)}., {first}",
                FullNameFormat.Initials => $"{Initial(first)}{Initial(last)}",
                FullNameFormat.InitialsDots => $"{Initial(first)}.{Initial(last)}.",
                _ => throw new NotImplementedException($"Enum not handled: {format}")
            };
        }

        private static string Initial(string name)//LATER make this handle double barrel names
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            return name.Substring(0, 1);
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
}

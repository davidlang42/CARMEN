using Carmen.CastingEngine;
using Carmen.ShowModel.Applicants;
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
    /// A converter which takes a whole Applicant object and produces a short summary.
    /// If the ApplicantEngine property is set, it will also include the Applicant's overall mark if available.
    /// </summary>
    public class ApplicantDescription : IValueConverter
    {
        public IApplicantEngine? ApplicantEngine { get; set; } = null;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Applicant a)
                return null;
            var elements = new List<string>();
            if (a.Gender.HasValue && a.DateOfBirth.HasValue)
            {
                var g = a.Gender.Value switch
                {
                    Gender.Male => "M",
                    Gender.Female => "F",
                    _ => throw new NotImplementedException($"Gender not handled: {a.Gender}")
                };
                elements.Add($"{a.Age}{g}");
            }
            if (a.CastGroup is CastGroup cg)
                elements.Add(cg.Abbreviation);
            if (a.CastNumber is int num)
                elements.Add($"#{num}");
            if (a.AlternativeCast is AlternativeCast ac)
                elements.Add(ac.Initial.ToString());
            if (ApplicantEngine is IApplicantEngine engine)
                elements.Add($"overall {engine.OverallAbility(a)}");
            return string.Join(" ", elements);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

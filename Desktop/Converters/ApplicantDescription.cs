using Carmen.CastingEngine;
using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Converters
{
    /// <summary>
    /// A converter which takes a whole Applicant object and produces a short summary.
    /// If the AuditionEngine property is set, it will also include the Applicant's overall mark if available.
    /// </summary>
    public class ApplicantDescription : IValueConverter
    {
        public IAuditionEngine? AuditionEngine { get; set; } = null;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Applicant a)
                return null;
            var elements = new List<string>();
            if (a.Gender.HasValue && a.DateOfBirth.HasValue)
                elements.Add($"{a.Age}{a.Gender.Value.ToChar()}");
            if (a.CastGroup is CastGroup cg)
                elements.Add(cg.Abbreviation);
            if (a.CastNumber is int num)
                elements.Add($"#{num}");
            if (a.AlternativeCast is AlternativeCast ac)
                elements.Add(ac.Initial.ToString());
            if (AuditionEngine is IAuditionEngine engine)
                elements.Add($"overall {engine.OverallAbility(a)}");
            return string.Join(" ", elements);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

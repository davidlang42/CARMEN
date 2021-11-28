using Carmen.CastingEngine.Audition;
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
    /// Calculates the overall ability of a supplied Applicant using the instance property AuditionEngine
    /// </summary>
    class OverallAbilityCalculator : IValueConverter
    {
        public IAuditionEngine? AuditionEngine { get; set; }

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Applicant applicant)
                return null;
            var engine = AuditionEngine ?? throw new ApplicationException($"{nameof(AuditionEngine)} not set.");
            return engine.OverallAbility(applicant);
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

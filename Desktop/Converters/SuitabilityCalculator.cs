using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.Selection;
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
    /// Convert {Applicant, CastGroup/Tag?} into a suitability percentage for that CastGroup/Tag.
    /// If anything other than CastGroup or Tag is the second value, the Applicant's overall ability is returned.
    /// </summary>
    public class SuitabilityCalculator : IMultiValueConverter
    {
        public ISelectionEngine? SelectionEngine { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] is not Applicant applicant)
                throw new ApplicationException("Values must be {Applicant, CastGroup/Tag?}");
            var suitability = Calculate(applicant, values[1]);
            return $"{suitability * 100:0}%";
        }

        public double Calculate(Applicant applicant, object? cast_group_or_tag)
        {
            var engine = SelectionEngine ?? throw new ApplicationException($"{nameof(SelectionEngine)} not set.");
            return cast_group_or_tag switch
            {
                CastGroup cg => engine.SuitabilityOf(applicant, cg),
                Tag t => engine.SuitabilityOf(applicant, t),
                _ => engine.OverallSuitability(applicant)
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

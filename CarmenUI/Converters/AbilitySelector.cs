using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// Selects an ability mark from a list of Abilities, for the set Criteria.
    /// </summary>
    public class AbilitySelector : IValueConverter
    {
        public Criteria Criteria { get; set; }

        public AbilitySelector(Criteria criteria)
        {
            Criteria = criteria;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IList list)
                throw new ArgumentException("Value must be an IList containing Abilities.");
            var ability = list.OfType<Ability>().Where(ab => ab.Criteria == Criteria).SingleOrDefault();
            if (ability == null)
                return null;
            return Criteria switch
            {
                NumericCriteria => ability.Mark,
                BooleanCriteria => ability.Mark == 0 ? "No" : "Yes",
                SelectCriteria select => select.Options[ability.Mark],
                _ => throw new NotImplementedException($"Criteria not handled: {Criteria.GetType().Name}")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

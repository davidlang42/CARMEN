﻿using ShowModel.Applicants;
using ShowModel.Criterias;
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
    /// Checks if a provided applicant has completed their audition.
    /// Values provided must be {IsRegistered, Abilities}.
    /// ConverterParameter must be a collection of all criterias.
    /// Return value is Visibility.Visible if true, otherwise Visibility.Collapsed.
    /// </summary>
    public class ShowIfApplicantRegisteredAndAuditioned : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ApplicationException("Values must be {IsRegistered, Abilities}");
            if (values[0] is not bool is_registered)
                return Visibility.Collapsed;
            if (values[1] is not IEnumerable<Ability> abilities)
                return Visibility.Collapsed;
            if (parameter is not CollectionViewSource view_source)
                throw new ArgumentException("ConverterParameter must be an CollectionViewSource containing all criterias.");
            return Check(is_registered, abilities, view_source) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool Check(bool is_registered, IEnumerable<Ability> applicant_abilities, CollectionViewSource criterias)
        {
            if (criterias.Source is not IList list)
                return false;
            return Check(is_registered, applicant_abilities, list.OfType<Criteria>());
        }

        public static bool Check(bool is_registered, IEnumerable<Ability> applicant_abilities, IEnumerable<Criteria> all_criterias)
        {
            if (!is_registered)
                return false;
            foreach (var ability in applicant_abilities)
                if (ability.Mark > ability.Criteria.MaxMark)
                    return false;
            foreach (var required_criteria in all_criterias.Where(c => c.Primary))
                if (!applicant_abilities.Any(ab => ab.Criteria == required_criteria))
                    return false;
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
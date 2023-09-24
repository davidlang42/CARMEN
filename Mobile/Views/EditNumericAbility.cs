using Carmen.Mobile.Models;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using CommunityToolkit.Maui.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditNumericAbility : EditAbility<NumericCriteria>
    {
        public EditNumericAbility(NumericCriteria criteria, Ability ability, Action delete)
            : base(criteria, ability, GenerateEditView(criteria), delete)
        { }

        static View GenerateEditView(NumericCriteria criteria)
        {
            var label = new Label { Text = criteria.Name };
            var entry = new Entry
            {
                Keyboard = Keyboard.Numeric,
            };
            entry.Behaviors.Add(new NumericValidationBehavior
            {
                MinimumValue = 0,
                MaximumValue = criteria.MaxMark,
                MaximumDecimalPlaces = 0
            });
            entry.SetBinding(Entry.TextProperty, new Binding(nameof(Ability.Mark)));
            return new VerticalStackLayout()
            {
                label,
                entry
            };
        }
    }
}

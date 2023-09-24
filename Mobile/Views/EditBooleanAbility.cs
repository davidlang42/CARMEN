using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditBooleanAbility : EditAbility<BooleanCriteria>
    {
        public EditBooleanAbility(BooleanCriteria criteria, Ability ability)
            : base(criteria, ability, GenerateEditView(criteria))
        { }

        static View GenerateEditView(BooleanCriteria criteria)
        {
            var checkbox = new CheckBox();
            checkbox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(Ability.Mark), converter: new MatchUIntValue(), converterParameter: 1));
            return new VerticalStackLayout
            {
                Padding = 5,
                Children = {
                    new Label { Text = criteria.Name },
                    checkbox
                }
            };
        }
    }
}

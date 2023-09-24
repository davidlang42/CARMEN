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
    internal class EditSelectAbility : EditAbility<SelectCriteria>
    {
        public EditSelectAbility(SelectCriteria criteria, Ability ability, Action delete)
            : base(criteria, ability, GenerateEditView(criteria), delete)
        { }

        static View GenerateEditView(SelectCriteria criteria)
        {
            var label = new Label { Text = criteria.Name };
            var picker = new Picker
            {
                ItemsSource = criteria.Options,
                ItemDisplayBinding = new Binding
                {
                    StringFormat = "{0} " // this avoids a completely blank string as an option, which gets displayed as "Microsoft.Maui.Picker"
                }
            };
            picker.SetBinding(Picker.SelectedIndexProperty, new Binding(nameof(Ability.Mark)));
            return new VerticalStackLayout()
            {
                label,
                picker
            };
        }
    }
}

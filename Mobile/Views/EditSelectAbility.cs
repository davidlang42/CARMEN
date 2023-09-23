﻿using Carmen.ShowModel.Applicants;
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
        public EditSelectAbility(SelectCriteria criteria, Ability ability)
            : base(criteria, ability, GenerateEditView(criteria))
        { }

        static View GenerateEditView(SelectCriteria criteria)
        {
            var label = new Label { Text = criteria.Name };
            var picker = new Picker
            {
                ItemsSource = criteria.Options //TODO if blank string is an option, it shows in the list as "Microsoft.Maui.Picker"
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
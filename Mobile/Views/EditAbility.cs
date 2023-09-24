using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditAbility<T> : EditBase
        where T: Criteria
    {
        protected readonly T criteria;
        protected readonly Ability ability;
        readonly Action delete;

        protected EditAbility(T criteria, Ability ability, View edit_view, Action delete)
            : base(ability.Applicant, edit_view)
        {
            this.criteria = criteria;
            this.ability = ability;
            this.delete = delete;
            BindingContext = ability;
        }

        protected override IEnumerable<View> GenerateButtons()
        {
             foreach (var button in base.GenerateButtons())
                yield return button;
            var clear = new Button
            {
                Text = "Clear"
            };
            clear.Clicked += Clear_Clicked;
            yield return clear;
        }

        private async void Clear_Clicked(object? sender, EventArgs e)
        {
            delete();
            await Navigation.PopAsync();
        }
    }
}

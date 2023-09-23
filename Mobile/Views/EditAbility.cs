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

        protected EditAbility(T criteria, Ability ability, View edit_view)
            : base(ability.Applicant, edit_view)
        {
            this.criteria = criteria;
            this.ability = ability;
            BindingContext = ability;
        }
    }
}

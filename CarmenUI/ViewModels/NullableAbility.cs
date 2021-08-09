using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class NullableAbility
    {
        ObservableCollection<Ability> collection;
        Ability ability;
        bool attached;

        public Criteria Criteria => ability.Criteria;

        public uint? Mark
        {
            get => attached ? ability.Mark : null;
            set
            {
                if (value == null)
                {
                    if (attached)
                        collection.Remove(ability);
                    attached = false;
                }
                else
                {
                    ability.Mark = value.Value;
                    if (!attached)
                        collection.Add(ability);
                    attached = true;
                }
            }
        }

        public NullableAbility(ObservableCollection<Ability> collection, Criteria criteria)
        {
            this.collection = collection;
            if (collection.Where(ab => ab.Criteria == criteria).FirstOrDefault() is Ability existing)
            {
                ability = existing;
                attached = true;
            }
            else
            {
                ability = new Ability { Criteria = criteria };
                attached = false;
            }
        }
    }
}

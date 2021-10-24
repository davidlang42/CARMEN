using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class NullableAbility : INotifyPropertyChanged
    {
        readonly ObservableCollection<Ability> collection;
        readonly Ability ability;
        bool attached;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                OnPropertyChanged();
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

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

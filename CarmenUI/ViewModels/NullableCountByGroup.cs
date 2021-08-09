using Carmen.ShowModel.Applicants;
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
    public class NullableCountByGroup : INotifyPropertyChanged
    {
        ICollection<CountByGroup> collection;
        CountByGroup countByGroup;
        bool attached;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CastGroup CastGroup => countByGroup.CastGroup;

        public uint? Count
        {
            get => attached ? countByGroup.Count : null;
            set
            {
                if (value == null)
                {
                    if (attached)
                        collection.Remove(countByGroup);
                    attached = false;
                }
                else
                {
                    countByGroup.Count = value.Value;
                    if (!attached)
                        collection.Add(countByGroup);
                    attached = true;
                }
                OnPropertyChanged();
            }
        }

        public NullableCountByGroup(ICollection<CountByGroup> collection, CastGroup cast_group)
        {
            this.collection = collection;
            if (collection.Where(cbg => cbg.CastGroup == cast_group).FirstOrDefault() is CountByGroup existing)
            {
                countByGroup = existing;
                attached = true;
            }
            else
            {
                countByGroup = new CountByGroup { CastGroup = cast_group };
                attached = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

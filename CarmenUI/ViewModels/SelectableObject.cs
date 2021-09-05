using Carmen.ShowModel.Requirements;
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
    public class SelectableObject<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICollection<T> Collection { get; init; }
        public T ObjectValue { get; init; }

        private Action<T>? additionalAddAction;
        private Action<T>? additionalRemoveAction;

        public bool IsSelected
        {
            get => Collection.Contains(ObjectValue);
            set
            {
                if (value == IsSelected)
                    return;
                if (value)
                {
                    Collection.Add(ObjectValue);
                    additionalAddAction?.Invoke(ObjectValue);
                }
                else
                {
                    Collection.Remove(ObjectValue);
                    additionalRemoveAction?.Invoke(ObjectValue);
                }
                OnPropertyChanged();
            }
        }

        public SelectableObject(ICollection<T> collection, T object_value, Action<T>? additional_add_action = null, Action<T>? additional_remove_action = null)
        {
            this.Collection = collection;
            this.ObjectValue = object_value;
            this.additionalAddAction = additional_add_action;
            this.additionalRemoveAction = additional_remove_action;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

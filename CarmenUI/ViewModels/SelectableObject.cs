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

        public bool IsSelected
        {
            get => Collection.Contains(ObjectValue);
            set
            {
                if (value == IsSelected)
                    return;
                if (value)
                    Collection.Add(ObjectValue);
                else
                    Collection.Remove(ObjectValue);
                OnPropertyChanged();
            }
        }

        public SelectableObject(ICollection<T> collection, T objectValue)
        {
            this.Collection = collection;
            this.ObjectValue = objectValue;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

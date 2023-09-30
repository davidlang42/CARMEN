using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class Roles : INotifyPropertyChanged
    {
        public bool IsLoading { get; private set; } = true;
        public bool IsEmpty { get; private set; }
        public ItemRole[]? Collection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property_name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public void Loaded(ItemRole[] collection)
        {
            Collection = collection;
            IsLoading = false;
            IsEmpty = collection.Length == 0;
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}

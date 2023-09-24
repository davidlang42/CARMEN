using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class Applicants : INotifyPropertyChanged
    {
        public bool IsLoading { get; private set; } = true;
        public ObservableCollection<Applicant>? Collection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Loaded(ObservableCollection<Applicant> collection)
        {
            Collection = collection;
            IsLoading = false;
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged(nameof(IsLoading));
        }

        public void Adding()
        {
            Collection = null;
            IsLoading = true;
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged(nameof(IsLoading));
        }
    }
}

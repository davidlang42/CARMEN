using Carmen.Mobile.Collections;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class Applicants : INotifyPropertyChanged
    {
        readonly Func<Applicant, string, bool> filterFunction;

        public bool IsLoading { get; private set; } = true;
        public FilteredSortedCollection<Applicant>? Collection { get; private set; }

        private string filterText = "";
        public string FilterText
        {
            get => filterText;
            set
            {
                if (filterText == value)
                    return;
                filterText = value;
                if (Collection != null)
                    Collection.Filter = value == "" ? null : a => filterFunction(a, value);
                OnPropertyChanged();
            }
        }

        public Applicants(Func<Applicant, string, bool> filter_function)
        {
            this.filterFunction = filter_function;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Loaded(Applicant[] collection, IComparer<Applicant> sort_by)
        {
            Collection = new FilteredSortedCollection<Applicant>(collection)
            {
                SortBy = sort_by
            };
            IsLoading = false;
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged(nameof(IsLoading));
        }

        public void Added(Applicant applicant)
        {
            Collection?.Add(applicant);
            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }

        public void Adding()
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
        }

        public void Removed(Applicant applicant)
        {
            Collection?.Remove(applicant);
        }
    }
}

﻿using Carmen.Mobile.Collections;
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
        public FilteredSortedCollection<Applicant>? Collection { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Loaded(Applicant[] collection)
        {
            Collection = new FilteredSortedCollection<Applicant>(collection);
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

using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class MainModel : INotifyPropertyChanged
    {
        public string PageTitle { get; private set; } = "CARMEN";

        public bool IsLoading => LoadingMessage != null;
        public string? LoadingMessage { get; private set; } = ""; // default state is loading with no message

        public bool IsError => ErrorMessage != null;
        public string? ErrorMessage { get; private set; }

        public bool IsReady => LoadingMessage == null && ErrorMessage == null;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void MessagesChanged()
        {
            OnPropertyChanged(nameof(LoadingMessage));
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsError));
            OnPropertyChanged(nameof(IsReady));
        }

        public void Loading(string message)
        {
            LoadingMessage = message;
            ErrorMessage = null;
            MessagesChanged();
        }

        public void Error(string message)
        {
            ErrorMessage = message;
            LoadingMessage = null;
            MessagesChanged();
        }

        public void Ready()
        {
            LoadingMessage = null;
            ErrorMessage = null;
            MessagesChanged();
        }

        public void SetShowName(string name)
        {
            PageTitle = $"CARMEN: {name}";
            OnPropertyChanged(nameof(PageTitle));
        }
    }
}

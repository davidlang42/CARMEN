using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    public class ImportColumn : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
            }
        }

        private InputColumn? selectedInput;
        public InputColumn? SelectedInput
        {
            get => selectedInput;
            set
            {
                if (selectedInput == value)
                    return;
                selectedInput = value;
                OnPropertyChanged();
            }
        }

        private bool matchExisting;
        public bool MatchExisting
        {
            get => matchExisting;
            set
            {
                if (matchExisting == value)
                    return;
                matchExisting = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Should throw ParseException if string value is invalid.</summary>
        public Action<Applicant, string> ValueSetter { get; }

        public ImportColumn(string name, Action<Applicant, string> setter, bool match_existing = false)
        {
            this.name = name;
            this.matchExisting = match_existing;
            ValueSetter = setter;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ApplicantField<T> : INotifyPropertyChanged, IApplicantField
    {
        public Applicant Applicant { get; }

        readonly Func<Applicant, T> getter;
        readonly Action<Applicant, T> setter;

        public string Label { get; }
        public T Value
        {
            get => getter(Applicant);
            set
            {
                setter(Applicant, value);
                OnPropertyChanged();
            }
        }

        object? IApplicantField.Value
        {
            get => Value;
            set
            {
                if (value is T typed)
                    Value = typed;
            }
        }

        public ApplicantField(string label, Func<Applicant, T> getter, Action<Applicant, T> setter, Applicant applicant)
        {
            Label = label;
            this.getter = getter;
            this.setter = setter;
            Applicant = applicant;
            applicant.PropertyChanged += Applicant_PropertyChanged;
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // this assume any applicant change is changing this field, which isn't true but isn't worth worrying about
            OnPropertyChanged(nameof(Value));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

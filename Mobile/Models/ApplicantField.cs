using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ApplicantField : INotifyPropertyChanged
    {
        readonly Applicant applicant;
        readonly Func<Applicant, object?> getter;
        readonly Action<Applicant, object?> setter;

        public string Label { get; }
        public object? Value
        {
            get => getter(applicant);
            set => setter(applicant, value);
        }
        
        private ApplicantField(string label, Func<Applicant, object?> getter, Action<Applicant, object?> setter, Applicant applicant)
        {
            Label = label;
            this.getter = getter;
            this.setter = setter;
            this.applicant = applicant;
            applicant.PropertyChanged += Applicant_PropertyChanged;
        }

        public static ApplicantField New<T>(string label, Func<Applicant, T> getter, Action<Applicant, T> setter, Applicant applicant)
        {
            return new(label, a => getter(a), (a, v) => setter(a, (T?)v), applicant);
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // this assume any applicant change is changing this field, which isn't true but isn't worth worrying about
            OnPropertyChanged(nameof(Value));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

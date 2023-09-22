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
        readonly ApplicantModel model;
        readonly Func<Applicant, object?> getter;
        readonly Action<Applicant, object?> setter;

        public string Label { get; }
        public object? Value
        {
            get
            {
                if (model.Applicant == null)
                    return null;
                return getter(model.Applicant);
            }
            set
            {
                if (model.Applicant == null)
                    return;
                setter(model.Applicant, value);
            }
        }
        
        private ApplicantField(string label, Func<Applicant, object?> getter, Action<Applicant, object?> setter, ApplicantModel model)
        {
            Label = label;
            this.getter = getter;
            this.setter = setter;
            this.model = model;
            model.PropertyChanged += Model_PropertyChanged;
        }

        public static ApplicantField New<T>(string label, Func<Applicant, T> getter, Action<Applicant, T> setter, ApplicantModel model)
        {
            return new(label, a => getter(a), (a, v) => setter(a, (T?)v), model);
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicantModel.Applicant))
            {
                OnPropertyChanged(nameof(Value));
                if (model.Applicant is Applicant applicant)
                    applicant.PropertyChanged += Applicant_PropertyChanged;
            }
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Value));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}

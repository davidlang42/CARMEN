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

        public string Label { get; }
        public object? Value => model.Applicant == null ? null : getter(model.Applicant);
        
        public ApplicantField(string label, Func<Applicant, object?> getter, ApplicantModel model)
        {
            Label = label;
            this.getter = getter;
            this.model = model;
            model.PropertyChanged += Model_PropertyChanged;
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

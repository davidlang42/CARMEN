using Carmen.Mobile.Converters;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ApplicantModel : INotifyPropertyChanged
    {
        public bool IsLoading { get; private set; } = true;
        public int ApplicantId { get; }
        public Applicant? Applicant { get; private set; }

        public ApplicantModel(int applicant_id)
        {
            ApplicantId = applicant_id;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Loaded(Applicant applicant)
        {
            Applicant = applicant;
            IsLoading = false;
            OnPropertyChanged(nameof(Applicant));
            OnPropertyChanged(nameof(IsLoading));
        }

        public string GetFullName()
            => (string)new FullName().Convert(new[] { Applicant?.FirstName, Applicant?.LastName }, null, null, null);
    }
}

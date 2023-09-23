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
        public IApplicantField[] Fields
        {
            get
            {
                if (Applicant is not Applicant applicant)
                    return Array.Empty<IApplicantField>();
                return new IApplicantField[] {
                    new ApplicantField<string>("First name", a => a.FirstName, (a, v) => a.FirstName = v, applicant),
                    new ApplicantField<string>("Last name", a => a.LastName, (a, v) => a.LastName = v, applicant),
                    new ApplicantField<Gender?>("Gender", a => a.Gender, (a, v) => a.Gender = v, applicant),
                    new ApplicantField<DateTime?>("Date of birth", a => a.DateOfBirth, (a, v) => a.DateOfBirth = v, applicant)
                };
            }
        }
        public string FullName
            => FullNameFormatter.Format(Applicant?.FirstName ?? originalFirstName, Applicant?.LastName ?? originalLastName);
        public bool IsLoadingPhoto { get; private set; } = true;
        public ImageSource? Photo { get; private set; }

        readonly string originalFirstName;
        readonly string originalLastName;

        public ApplicantModel(int applicant_id, string original_first_name, string original_last_name)
        {
            ApplicantId = applicant_id;
            originalFirstName = original_first_name;
            originalLastName = original_last_name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Loaded(Applicant applicant)
        {
            Applicant = applicant;
            applicant.PropertyChanged += Applicant_PropertyChanged; ;
            IsLoading = false;
            OnPropertyChanged(nameof(Applicant));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Fields));
            OnPropertyChanged(nameof(IsLoading));
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FullName));
        }

        public void LoadedPhoto(ImageSource? photo)
        {
            Photo = photo;
            IsLoadingPhoto = false;
            OnPropertyChanged(nameof(Photo));
            OnPropertyChanged(nameof(IsLoadingPhoto));
        }

        public static string Path(string applicant_field_path) => $"{nameof(Applicant)}.{applicant_field_path}";
    }
}

using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CarmenUI.ViewModels
{
    public class CastNumber : INotifyPropertyChanged
    {
        AlternativeCast[] alternativeCasts;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int number;
        public int Number
        {
            get => number;
            set
            {
                if (number == value)
                    return;
                number = value;
                OnPropertyChanged();
            }
        }

        private Applicant?[] applicants;
        public ReadOnlyCollection<Applicant?> Applicants => Array.AsReadOnly(applicants);

        public bool IsEmpty => Applicants.All(a => a == null);

        public CastNumber(IEnumerable<Applicant> applicants_of_the_same_cast_number, AlternativeCast[] alternative_casts)
        {
            alternativeCasts = alternative_casts;
            var e = applicants_of_the_same_cast_number.GetEnumerator();
            if (!e.MoveNext())
                throw new InvalidOperationException($"{nameof(applicants_of_the_same_cast_number)} is empty");
            number = e.Current.CastNumber ?? throw new ApplicationException("Applicant cast number not set.");
            applicants = Array.Empty<Applicant?>();
            do
            {
                Add(e.Current);
            } while (e.MoveNext());
        }

        public void Add(Applicant applicant)
        {
            if (applicant.CastNumber != number)
                throw new ApplicationException("Tried to add an applicant with the wrong cast number.");
            if (IsEmpty)
            {
                var cast_group = applicant.CastGroup ?? throw new ApplicationException("Tried to add an applicant with no cast group.");
                if (cast_group.AlternateCasts)
                    applicants = new Applicant?[alternativeCasts.Length];
                else
                {
                    if (applicant.AlternativeCast != null)
                        throw new ApplicationException("Applicant has an alternative cast but is in a cast group which does not alternate.");
                    applicants = new Applicant?[] { applicant };
                    OnPropertyChanged(nameof(Applicants));
                    return;
                }
            }
            if (applicant.AlternativeCast == null)
                throw new ApplicationException("Tried to add an applicant to an alternating cast number without an alternative cast.");
            var index = Array.IndexOf(alternativeCasts, applicant.AlternativeCast);
            if (applicants[index] != null)
                throw new ApplicationException($"Cast number {number} already contains an applicant in {applicant.AlternativeCast.Name}");
            applicants[index] = applicant;
            OnPropertyChanged(nameof(Applicants));
        }

        public void Remove(Applicant applicant)
        {
            for (var i = 0; i < applicants.Length; i++)
                if (applicants[i] == applicant)
                {
                    applicants[i] = null;
                    OnPropertyChanged(nameof(Applicants));
                    return;
                }
            throw new ApplicationException("Applicant not found.");
        }

        public bool Contains(Applicant applicant)
        {
            for (var i = 0; i < applicants.Length; i++)
                if (applicants[i] == applicant)
                    return true;
            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

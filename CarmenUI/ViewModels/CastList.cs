using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class CastList
    {
        Applicant[] allApplicants;
        AlternativeCast[] alternativeCasts;

        public ObservableCollection<Applicant> MissingNumbers { get; } = new();
        public ObservableCollection<CastNumber> CastNumbers { get; } = new();

        public CastList(Applicant[] all_applicants, AlternativeCast[] alternative_casts)
        {
            allApplicants = all_applicants;
            alternativeCasts = alternative_casts;
            foreach (var group in all_applicants.Where(a => a.CastGroup != null).GroupBy(a => a.CastNumber))
            {
                if (group.Key == null)
                    foreach (var applicant in group)
                        MissingNumbers.Add(applicant);
                else
                    CastNumbers.Add(new CastNumber(group.ToArray(), alternative_casts));
            }
            foreach (var applicant in allApplicants)
                applicant.PropertyChanged += Applicant_PropertyChanged;
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e) //TODO dispose handlers
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Applicant.CastNumber)
                || e.PropertyName == nameof(Applicant.CastGroup) || e.PropertyName == nameof(Applicant.AlternativeCast))
            {
                // Any all times an applicant will either be in:
                // - MissingNumbers, if their cast number isn't set
                // - CastNumbers, if their cast number is set
                // - neither, if they aren't selected into a valid CastGroup/AlternativeCast
                var applicant = sender as Applicant ?? throw new ApplicationException("Sender not set to applicant.");
                if (MissingNumbers.Contains(applicant))
                    MissingNumbers.Remove(applicant);
                else
                    RemoveFromCastNumbersIfPresent(applicant);
                if (applicant.CastGroup is CastGroup cg && (applicant.AlternativeCast == null) != cg.AlternateCasts)
                {
                    if (applicant.CastNumber.HasValue)
                        AddToCastNumbers(applicant);
                    else
                        MissingNumbers.Add(applicant);
                }
            }
        }

        private void RemoveFromCastNumbersIfPresent(Applicant applicant)
        {
            if (CastNumbers.Where(n => n.Contains(applicant)).SingleOrDefault() is CastNumber existing)
            {
                existing.Remove(applicant);
                if (existing.IsEmpty)
                    CastNumbers.Remove(existing);
            }
        }

        private void AddToCastNumbers(Applicant applicant)
        {
            if (CastNumbers.Where(n => n.Number == applicant.CastNumber).SingleOrDefault() is CastNumber existing)
                existing.Add(applicant);
            else
                CastNumbers.Add(new CastNumber(applicant.Yield(), alternativeCasts));
        }
    }
}

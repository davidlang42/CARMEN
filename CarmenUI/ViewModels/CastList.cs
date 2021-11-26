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
        public AlternativeCast[] AlternativeCasts { get; }
        public ObservableCollection<Applicant> MissingNumbers { get; } = new();
        public ObservableCollection<CastNumber> CastNumbers { get; } = new();

        public CastList(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts)
        {
            AlternativeCasts = alternative_casts;
            foreach (var group in applicants.Where(a => a.CastGroup != null).GroupBy(a => a.CastNumber))
            {
                if (group.Key == null)
                    foreach (var applicant in group)
                    {
                        applicant.PropertyChanged += Applicant_PropertyChanged;
                        MissingNumbers.Add(applicant);
                    }
                else
                    CastNumbers.Add(new CastNumber(group.Wrap(a => a.PropertyChanged += Applicant_PropertyChanged).ToArray(), alternative_casts));
            }
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Applicant.CastNumber))
            {
                var applicant = sender as Applicant ?? throw new ApplicationException("Sender not set to applicant.");
                if (MissingNumbers.Contains(applicant))
                    MissingNumbers.Remove(applicant);
                else
                    RemoveFromCastNumbers(applicant);
                if (applicant.CastNumber.HasValue)
                    AddToCastNumbers(applicant);
                else
                    MissingNumbers.Add(applicant);
            }
        }

        private void RemoveFromCastNumbers(Applicant applicant)
        {
            var existing = CastNumbers.Where(n => n.Contains(applicant)).Single();
            existing.Remove(applicant);
            if (existing.IsEmpty)
                CastNumbers.Remove(existing);
        }

        private void AddToCastNumbers(Applicant applicant)
        {
            if (CastNumbers.Where(n => n.Number == applicant.CastNumber).SingleOrDefault() is CastNumber existing)
                existing.Add(applicant);
            else
                CastNumbers.Add(new CastNumber(applicant.Yield(), AlternativeCasts));
        }
    }
}

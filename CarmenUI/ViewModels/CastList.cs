using Carmen.CastingEngine.Selection;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class CastList : INotifyPropertyChanged
    {
        bool ignoreApplicantChanges = false;
        Applicant[] allApplicants;
        
        public AlternativeCast[] AlternativeCasts { get; }
        public ObservableCollection<Applicant> MissingNumbers { get; } = new();
        public ObservableCollection<CastNumber> CastNumbers { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool ContainsGaps
        {
            get
            {
                if (CastNumbers.Count == 0)
                    return false;
                var max = CastNumbers.Max(n => n.Number);
                for (var i = CastNumberSet.FIRST_CAST_NUMBER; i < max; i++)
                    if (!CastNumbers.Where(n => n.Number == i).Any())
                        return true;
                return false;
            }
        }

        public CastList(Applicant[] all_applicants, AlternativeCast[] alternative_casts)
        {
            allApplicants = all_applicants;
            AlternativeCasts = alternative_casts;
            CastNumbers.CollectionChanged += CastNumbers_CollectionChanged;
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

        private void CastNumbers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            OnPropertyChanged(nameof(ContainsGaps)); // maybe
            if (e.OldItems != null)
                foreach (var removed_item in e.OldItems.OfType<CastNumber>())
                    removed_item.PropertyChanged -= CastNumber_PropertyChanged;
            if (e.NewItems != null)
                foreach (var added_item in e.NewItems.OfType<CastNumber>())
                    added_item.PropertyChanged += CastNumber_PropertyChanged;
        }

        private void CastNumber_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CastNumber.Number))
                OnPropertyChanged(nameof(ContainsGaps)); // maybe
        }

        private void Applicant_PropertyChanged(object? sender, PropertyChangedEventArgs e) //TODO dispose handlers
        {
            if (ignoreApplicantChanges)
                return;
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
                CastNumbers.Add(new CastNumber(applicant.Yield(), AlternativeCasts));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void FillGaps()
        {
            var i = CastNumberSet.FIRST_CAST_NUMBER;
            var new_cast_numbers = CastNumbers.OrderBy(n => n.Number).Select(n => (i++, n.Applicants.NotNull().ToArray())).ToArray();
            foreach (var (new_cast_number, applicants) in new_cast_numbers)
                foreach (var applicant in applicants)
                    applicant.CastNumber = new_cast_number;
        }

        public void Merge(IEnumerable<CastNumber> cast_numbers)
        {
            //TODO
        }

        public IEnumerable<CastNumber> Split(CastNumber cast_number)
        {
            var applicants = cast_number.Applicants.NotNull().ToArray();
            for (var i = 0; i < applicants.Length - 1; i++)
            {
                cast_number.Remove(applicants[i]);
                ShiftDown(cast_number);
                var new_cast_number = new CastNumber(applicants[i].Yield(), AlternativeCasts);
                CastNumbers.Add(new_cast_number);
                yield return new_cast_number;
            }
            yield return cast_number; // still contains the last applicant
        }

        private void ShiftDown(CastNumber cast_number)
        {
            var new_cast_number = cast_number.Number + 1;
            if (CastNumbers.Where(n => n.Number == new_cast_number).SingleOrDefault() is CastNumber existing)
                ShiftDown(existing);
            SetCastNumber(cast_number, new_cast_number);
        }

        public void MoveUp(CastNumber cast_number, int dont_move_above = CastNumberSet.FIRST_CAST_NUMBER)
        {
            if (cast_number.Number <= dont_move_above)
                return; // nothing to do
            var new_cast_number = cast_number.Number - 1;
            if (CastNumbers.Where(n => n.Number == new_cast_number).SingleOrDefault() is CastNumber existing)
                SwapCastNumbers(cast_number, existing);
            else
                SetCastNumber(cast_number, new_cast_number);
        }

        public void MoveDown(CastNumber cast_number)
        {
            // allow move down to create gaps
            var new_cast_number = cast_number.Number + 1;
            if (CastNumbers.Where(n => n.Number == new_cast_number).SingleOrDefault() is CastNumber existing)
                SwapCastNumbers(cast_number, existing);
            else
                SetCastNumber(cast_number, new_cast_number);
        }

        private void SwapCastNumbers(CastNumber a, CastNumber b)
        {
            var old_a = a.Number;
            SetCastNumber(a, b.Number);
            SetCastNumber(b, old_a);
        }

        private void SetCastNumber(CastNumber cast_number, int new_cast_number)
        {
            ignoreApplicantChanges = true;
            foreach (var applicant in cast_number.Applicants.NotNull().ToArray())
                applicant.CastNumber = new_cast_number;
            ignoreApplicantChanges = false;
            cast_number.Number = new_cast_number;
        }
    }
}

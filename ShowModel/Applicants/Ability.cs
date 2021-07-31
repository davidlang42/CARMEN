using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ShowModel.Criterias;

namespace ShowModel.Applicants
{
    /// <summary>
    /// The assessed ability of an applicant in a certain criteria.
    /// </summary>
    public class Ability : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Applicant applicant = null!;
        public virtual Applicant Applicant
        {
            get => applicant;
            set
            {
                if (applicant == value)
                    return;
                applicant = value;
                OnPropertyChanged();
            }
        }

        private Criteria criteria = null!;
        public virtual Criteria Criteria
        {
            get => criteria;
            set
            {
                if (criteria == value)
                    return;
                criteria = value;
                OnPropertyChanged();
            }
        }

        private uint mark;
        public uint Mark
        {
            get => mark;
            set
            {
                if (value > Criteria.MaxMark)
                    value = Criteria.MaxMark;
                if (mark == value)
                    return;
                mark = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

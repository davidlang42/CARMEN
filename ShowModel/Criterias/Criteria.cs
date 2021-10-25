using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Carmen.ShowModel.Applicants;

namespace Carmen.ShowModel.Criterias
{
    /// <summary>
    /// A criteria on which applicants are assessed, and assigned a mark.
    /// </summary>
    public abstract class Criteria : IOrdered, INamed, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int CriteriaId { get; private set; }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
            }
        }

        private bool required;
        public bool Required
        {
            get => required;
            set
            {
                if (required == value)
                    return;
                required = value;
                OnPropertyChanged();
            }
        }

        private bool primary;
        public bool Primary
        {
            get => primary;
            set
            {
                if (primary == value)
                    return;
                primary = value;
                OnPropertyChanged();
            }
        }

        private int order;
        public int Order
        {
            get => order;
            set
            {
                if (order == value)
                    return;
                order = value;
                OnPropertyChanged();
            }
        }

        private double weight = 1;
        public double Weight
        {
            get => weight;
            set
            {
                if (weight == value)
                    return;
                weight = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Ability> abilities = new();
        public virtual ICollection<Ability> Abilities => abilities;

        private uint maxMark;
        public virtual uint MaxMark
        {
            get => maxMark;
            set
            {
                if (value == 0)
                    throw new ArgumentException($"{nameof(MaxMark)} cannot be set to 0.");
                if (maxMark == value)
                    return;
                maxMark = value;
                OnPropertyChanged();
            }
        }

        public abstract string Format(uint mark);

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

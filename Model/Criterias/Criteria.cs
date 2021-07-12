using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Model.Applicants;

namespace Model.Criterias
{
    /// <summary>
    /// A criteria on which applicants are assessed, and assigned a mark.
    /// </summary>
    public abstract class Criteria : IOrdered
    {
        [Key]
        public int CriteriaId { get; private set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Order { get; set; }
        [Range(-1000,1000)]
        public double Weight { get; set; }
        public virtual ICollection<Ability> Abilities { get; private set; } = new ObservableCollection<Ability>();
        protected uint maxMark;
        [Range(0, uint.MaxValue)]
        public virtual uint MaxMark
        {
            get => maxMark;
            set
            {
                if (value == 0)
                    throw new ArgumentException($"{nameof(MaxMark)} cannot be set to 0.");
                var max_ability = Abilities.Select(a => a.Mark).DefaultIfEmpty().Max();
                if (max_ability > value)
                {
                    var max_applicant = Abilities.Where(a => a.Mark == max_ability).First().Applicant;
                    throw new ArgumentException($"{nameof(MaxMark)} cannot be set to {value} as {max_applicant.DisplayName()}'s mark for {Name} is set to {max_ability}.");
                }
                maxMark = value;
            }
        }
    }
}

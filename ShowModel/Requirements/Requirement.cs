using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Carmen.ShowModel.Requirements
{
    /// <summary>
    /// A requirement which can be satisfied by an Applicant
    /// </summary>
    public abstract class Requirement : IOrdered, IValidatable, INamed, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int RequirementId { get; private set; }

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

        private double suitabilityWeight;
        public double SuitabilityWeight
        {
            get => suitabilityWeight;
            set
            {
                if (suitabilityWeight == value)
                    return;
                suitabilityWeight = value;
                OnPropertyChanged();
            }
        }

        private double existingRoleWeight;
        public double ExistingRoleWeight//TODO probably only need this on ICrtieriaRequirement
        {
            get => existingRoleWeight;
            set
            {
                if (existingRoleWeight == value)
                    return;
                existingRoleWeight = value;
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

        private string? reason;
        public string? Reason
        {
            get => reason;
            set
            {
                if (reason == value)
                    return;
                reason = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Role> usedByRoles = new();
        public virtual ICollection<Role> UsedByRoles => usedByRoles;

        private ObservableCollection<CastGroup> usedByCastGroups = new();
        public virtual ICollection<CastGroup> UsedByCastGroups => usedByCastGroups;

        private ObservableCollection<CombinedRequirement> usedByCombinedRequirements = new();
        public virtual ICollection<CombinedRequirement> UsedByCombinedRequirements => usedByCombinedRequirements;

        private ObservableCollection<Tag> usedByTags = new();
        public virtual ICollection<Tag> UsedByTags => usedByTags;

        /// <summary>Checks if an Applicant satisfies this requirement.</summary>
        public abstract bool IsSatisfiedBy(Applicant applicant);

        public virtual bool IsSatisfiedBy(Applicant applicant, ICollection<Requirement> accumulate_failed_requirements)
        {
            var result = IsSatisfiedBy(applicant);
            if (!result)
                accumulate_failed_requirements.Add(this);
            return result;
        }

        /// <summary>Recursively visit all requirements which are referenced directly
        /// or indirectly by this requirement, checking for a circular reference.</summary>
        internal virtual bool HasCircularReference(HashSet<Requirement> path, HashSet<Requirement> visited)
        {
            visited.Add(this);
            return false;
        }

        /// <summary>Recursively enumerate all Requirements which are referenced directly or indirectly by this requirement</summary>
        public IEnumerable<Requirement> References()
        {
            var visited = new HashSet<Requirement>();
            var path = new HashSet<Requirement>();
            if (HasCircularReference(path, visited))
                throw new ApplicationException($"Requirement has a circular reference: ({string.Join(", ", path.Select(r => r.Name))})");
            return visited;
        }

        /// <summary>Checks if this requirement has a circular reference.</summary>
        public IEnumerable<string> Validate()
        {
            var visited = new HashSet<Requirement>();
            var path = new HashSet<Requirement>();
            if (HasCircularReference(path, visited))
                yield return $"Requirement has a circular reference: ({string.Join(", ", path.Select(r => r.Name))})";
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

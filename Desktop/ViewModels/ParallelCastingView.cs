using Carmen.CastingEngine.Allocation;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelCastingView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title => $"Parallel casting {Roles.Length} roles in {Node.Name}";

        public Node Node { get; }

        public ParallelRole[] Roles { get; }

        private ParallelRole? selectedRole = null;
        public ParallelRole? SelectedRole
        {
            get => selectedRole;
            set
            {
                if (selectedRole == value)
                    return;
                selectedRole = value;
                OnPropertyChanged();
            }
        }

        public ParallelApplicant[] Applicants { get; }

        public ParallelCastingView(IAllocationEngine engine, Node node, IEnumerable<Role> roles, IEnumerable<Applicant> applicants, Criteria[] primary_criterias)
        {
            Node = node;
            Roles = roles.Select(r => new ParallelRole(r)).ToArray();
            //requiredCastGroups = role.CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            Applicants = applicants.AsParallel().Select(a =>
            {
                var afrs = new Dictionary<ParallelRole, ApplicantForRole>();
                foreach (var pr in Roles)
                {
                    afrs.Add(pr, new ApplicantForRole(engine, a, pr.Role, primary_criterias));
                }
                //av.PropertyChanged += ApplicantForRole_PropertyChanged;
                return new ParallelApplicant(this, a, afrs);
            }).ToArray();
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            //view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            //ConfigureFiltering(show_unavailable, show_ineligible, show_unneeded);
            //ConfigureSorting(new[] { (nameof(ApplicantForRole.Suitability), ListSortDirection.Descending) });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

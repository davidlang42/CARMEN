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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

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

        public Canvas Canvas { get; } = new Canvas();

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
                var pa = new ParallelApplicant(this, a, afrs);
                pa.PropertyChanged += ParallelApplicant_PropertyChanged;
                return pa;
            }).ToArray();
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            //view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            //ConfigureFiltering(show_unavailable, show_ineligible, show_unneeded);
            //ConfigureSorting(new[] { (nameof(ApplicantForRole.Suitability), ListSortDirection.Descending) });
        }

        private void ParallelApplicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ParallelApplicant.SelectedForRoles))
            {
                UpdateLines();
            }
        }

        public void UpdateLines()
        {
            Canvas.Children.Clear();
            var r = new Random();
            for (var i = 0; i< 5; i++)
            {
                Canvas.Children.Add(new Line
                {
                    X1 = r.Next(1, 1000),
                    X2 = r.Next(1, 1000),
                    Y1 = r.Next(1, 1000),
                    Y2 = r.Next(1, 1000),
                    Stroke = new SolidColorBrush
                    {
                        Color = Colors.Black
                    }
                });
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

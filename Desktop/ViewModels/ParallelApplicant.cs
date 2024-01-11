using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelApplicant : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        readonly ParallelCastingView castingView;
        readonly Dictionary<ParallelRole, ApplicantForRole> applicantForRoles;

        public Applicant Applicant { get; }

        public ApplicantForRole? SelectedRole => castingView.SelectedRole is ParallelRole role ? applicantForRoles[role] : null;

        public ContextMenu? ContextMenu { get; }

        public IEnumerable<ParallelRole> SelectedForRoles => applicantForRoles.Where(kvp => kvp.Value.IsSelected).Select(kvp => kvp.Key);

        public ParallelApplicant(ParallelCastingView casting_view, Applicant applicant, Dictionary<ParallelRole, ApplicantForRole> applicant_for_roles)
        {
            castingView = casting_view;
            Applicant = applicant;
            applicantForRoles = applicant_for_roles;
            foreach (var afr in applicantForRoles.Values)
            {
                afr.PropertyChanged += ApplicantForRole_PropertyChanged;
            }
            castingView.PropertyChanged += CastingView_PropertyChanged;
            //Action? double_click = null, IEnumerable<(string, Action)>? right_click = null
            //DoubleClick = double_click;
            //if (right_click != null)
            //{
            //    ContextMenu = new ContextMenu();
            //    foreach (var (text, action) in right_click)
            //    {
            //        var menu_item = new MenuItem() { Header = text };
            //        menu_item.Click += (s, e) => action();
            //        ContextMenu.Items.Add(menu_item);
            //    }
            //}
        }

        private void ApplicantForRole_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ApplicantForRole.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedForRoles));
            }
        }

        private void CastingView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ParallelCastingView.SelectedRole))
            {
                OnPropertyChanged(nameof(SelectedRole));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

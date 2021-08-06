using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleWithApplicantsView : RoleView
    {
        public CastGroup[] CastGroups { get; init; }

        public uint[] SelectedCast { get; }//TODO

        public ObservableCollection<ApplicantForRole> Applicants { get; init; } = new();

        public RoleWithApplicantsView(Role role, CastGroup[] cast_groups, Criteria[] criterias, ICollection<Applicant> applicants)
            : base (role, cast_groups)
        {
            CastGroups = cast_groups;
            Applicants = new ObservableCollection<ApplicantForRole>(applicants.Select(a =>
            {
                var av = new ApplicantForRole(a, role, criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;//TODO is this really needed?
                return av;
            }));
            Applicants.CollectionChanged += Applicants_CollectionChanged;
        }

        private void Applicants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (ApplicantForRole av in e.OldItems)
                        {
                            av.PropertyChanged -= ApplicantForRole_PropertyChanged;
                            Role.Cast.Remove(av.Applicant);
                        }
                    if (e.NewItems != null)
                        foreach (ApplicantForRole av in e.NewItems)
                        {
                            Role.Cast.Add(av.Applicant);
                            av.PropertyChanged += ApplicantForRole_PropertyChanged;//TODO dispose handlers
                        }
                    break;
                case NotifyCollectionChangedAction.Reset://LATER is this implementation correct? probably isn't used
                    foreach (var av in Applicants)
                        av.PropertyChanged -= ApplicantForRole_PropertyChanged;
                    Role.Cast.Clear();
                    foreach (var av in Applicants)
                    {
                        Role.Cast.Add(av.Applicant);
                        av.PropertyChanged += ApplicantForRole_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // do nothing
                    break;
                default:
                    throw new NotImplementedException($"Action not handled: {e.Action}");
            }
        }
        private void ApplicantForRole_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "TODO") //TODO
            {
                
            }
        }
    }
}

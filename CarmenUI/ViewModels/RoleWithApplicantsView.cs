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
        public CastGroupAndCast[] CastGroupsByCast { get; init; }

        /// <summary>Indicies match CastGroupsByCast</summary>
        public uint[] RequiredCast { get; init; }

        /// <summary>Indicies match CastGroupsByCast</summary>
        public uint[] SelectedCast //LATER check if re-calculating this each time is too slow, maybe cache and update on each applicant change
        {
            get
            {
                var dictionary = Applicants.Where(a => a.IsSelected).GroupBy(a => a.CastGroupAndCast).ToDictionary(g => g.Key, g => (uint)g.Count());
                var result = new uint[CastGroupsByCast.Length];
                for (var i = 0; i < CastGroupsByCast.Length; i++)
                    if (dictionary.TryGetValue(CastGroupsByCast[i], out var count))
                        result[i] = count;
                return result;
            }
        }

        public ObservableCollection<ApplicantForRole> Applicants { get; init; } = new();

        public RoleWithApplicantsView(Role role, CastGroupAndCast[] cast_groups_by_cast, Criteria[] criterias, ICollection<Applicant> applicants)
            : base(role)
        {
            CastGroupsByCast = cast_groups_by_cast;
            RequiredCast = new uint[CastGroupsByCast.Length];
            for (var i = 0; i < CastGroupsByCast.Length; i++)
                RequiredCast[i] = role.CountFor(CastGroupsByCast[i].CastGroup);
            Applicants = new ObservableCollection<ApplicantForRole>(applicants.Select(a =>
            {
                var av = new ApplicantForRole(a, role, criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;//TODO is this really needed? i dont think we care
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

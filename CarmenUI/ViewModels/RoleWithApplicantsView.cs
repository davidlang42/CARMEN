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

        public RoleWithApplicantsView(Role role, CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Criteria[] criterias, ICollection<Applicant> applicants)
            : base (role, cast_groups)
        {
            CastGroupsByCast = CastGroupAndCast.Enumerate(cast_groups, alternative_casts).ToArray();
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

    public struct CastGroupAndCast
    {
        public CastGroup CastGroup { get; set; }
        public AlternativeCast? Cast { get; set; }

        public string Name => Cast == null ? CastGroup.Name : $"{CastGroup.Name} ({Cast.Name})";
        public string Abbreviation => Cast == null ? CastGroup.Abbreviation : $"{CastGroup.Abbreviation}-{Cast.Initial}";

        public CastGroupAndCast(CastGroup cast_group, AlternativeCast? alternative_cast = null)
        {
            CastGroup = cast_group;
            Cast = alternative_cast;
        }

        public CastGroupAndCast(Applicant applicant)
            : this(applicant.CastGroup ?? throw new ApplicationException("Applicant does not have a CastGroup."),
                  applicant.AlternativeCast)
        { }

        public static IEnumerable<CastGroupAndCast> Enumerate(CastGroup[] cast_groups, AlternativeCast[] alternative_casts)
        {
            foreach (var cast_group in cast_groups)
            {
                if (cast_group.AlternateCasts)
                    foreach (var alternative_cast in alternative_casts)
                        yield return new CastGroupAndCast(cast_group, alternative_cast);
                else
                    yield return new CastGroupAndCast(cast_group);
            }
        }
    }
}

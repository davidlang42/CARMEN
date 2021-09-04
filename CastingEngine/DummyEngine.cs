using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyApplicantEngine : ApplicantEngine
    {
        /// <summary>Dummy value is always 100</summary>
        public override int MaxOverallAbility => 100;

        /// <summary>Dummy value is always 0</summary>
        public override int MinOverallAbility => 0;

        /// <summary>Dummy value is the weighted average of abilities, not checking for any missing values</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));
    }

    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummySelectionEngine : SelectionEngine
    {
        public DummySelectionEngine(IApplicantEngine applicant_engine, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(applicant_engine, cast_number_order_by, cast_number_order_direction)
        { }

        /// <summary>Dummy implementation selects the required number of cast in each group, in random order, overwriting every applicants existing cast group</summary>
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts)
        {
            var e = applicants.GetEnumerator();
            foreach (var cast_group in cast_groups)
            {
                var required = cast_group.RequiredCount;
                if (cast_group.AlternateCasts)
                    required *= number_of_alternative_casts;
                for (var i=0; i<required; i++)
                {
                    if (!e.MoveNext())
                        return;
                    e.Current.CastGroup = cast_group;
                }
            }
            while (e.MoveNext())
                e.Current.CastGroup = null;
        }

        /// <summary>Dummy implementation allocates alternative casts randomly, overwriting every applicants existing alternative cast, ignoring same_cast_sets</summary>
        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets)
        {
            var cast_groups = applicants.Select(a => a.CastGroup).OfType<CastGroup>().ToHashSet();
            foreach (var cast_group in cast_groups)
            {
                var group_casts = cast_group.AlternateCasts ? alternative_casts : new AlternativeCast?[] { null };
                int ac = 0;
                foreach (var applicant in cast_group.Members)
                {
                    applicant.AlternativeCast = group_casts[ac++];
                    if (ac >= group_casts.Length)
                        ac = 0;
                }
            }
        }

        /// <summary>Dummy implementation allocates cast numbers for each cast group in order, in a random order, ignoring order_by</summary>
        public override void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction)
        {
            int num = 1;
            var cast_groups = applicants.Select(a => a.CastGroup).OfType<CastGroup>().ToHashSet();
            foreach (var cast_group in cast_groups)
            {
                var applicants_by_cast = cast_group.Members.GroupBy(a => a.AlternativeCast).Select(g => g.ToArray()).ToList();
                var common_length = applicants_by_cast.First().Length;
                if (!applicants_by_cast.All(arr => arr.Length == common_length))
                    throw new ArgumentException($"Alternative casts of {cast_group.Name} do not contain the same number of applicants.");
                for (var i = 0; i < common_length; i++)
                {
                    foreach (var arr in applicants_by_cast)
                        arr[i].CastNumber = num;
                    num++;
                }
            }
        }

        /// <summary>Dummy implementation applies tags to cast randomly, overwriting any tags previously applied to an applicant, ignoring requriements and failing to take into account alternative casts</summary>
        public override void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts)
        {
            foreach (var tag in tags)
            {
                tag.Members.Clear();
                foreach (var cbg in tag.CountByGroups)
                    foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup).Take((int)cbg.Count))
                        tag.Members.Add(applicant);
            }
        }
    }

    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyAllocationEngine : AllocationEngine
    {
        public DummyAllocationEngine(IApplicantEngine applicant_engine)
            : base(applicant_engine)
        { }

        /// <summary>Dummy value is the basic implementation of recursive requirements.
        /// This assumes there are no circular references.</summary>
        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            var sub_suitabilities = role.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average();
        }

        /// <summary>Dummy value counts top level AbilityExact/AbilityRange requirements only</summary>
        public override double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role)
            => applicant.Roles.Where(r => r != excluding_role)
            .Where(r => r.Requirements.Any(req => req is ICriteriaRequirement cr && cr.Criteria == criteria))
            .Count();

        /// <summary>Dummy selection picks the number of required applicants of each type from the list of applicants, ignoring availability and eligibility.
        /// This does not take into account people already cast in the role.</summary>
        public override IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts)
        {
            var casts = alternative_casts.ToArray();
            foreach (var cbg in role.CountByGroups)
            {
                if (cbg.CastGroup.AlternateCasts)
                {
                    foreach (var cast in casts)
                        foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup && a.AlternativeCast == cast).Take((int)cbg.Count))
                            yield return applicant;
                }
                else
                {
                    foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup).Take((int)cbg.Count))
                        yield return applicant;
                }
            }
        }
    }
}

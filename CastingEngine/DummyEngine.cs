using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyEngine : ICastingEngine
    {
        /// <summary>Dummy value is the basic implementation of recursive requirements.
        /// This assumes there are no circular references.</summary>
        public double SuitabilityOf(Applicant applicant, Role role)
        {
            var sub_suitabilities = role.Requirements.Select(req => SuitabilityOf(applicant, req));
            return sub_suitabilities.Average();
        }

        private double SuitabilityOf(Applicant applicant, Requirement requirement)
            => requirement switch
            {
                AbilityRangeRequirement arr => ScaledSuitability(arr.IsSatisfiedBy(applicant), arr.ScaleSuitability, applicant.MarkFor(arr.Criteria), arr.Criteria.MaxMark),
                NotRequirement nr => 1 - SuitabilityOf(applicant, nr.SubRequirement),
                AndRequirement ar => ar.AverageSuitability ? ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Product(),
                OrRequirement or => or.AverageSuitability ? or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Max(),
                XorRequirement xr => xr.SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe() is Requirement req ? SuitabilityOf(applicant, req) : 0,
                Requirement req => req.IsSatisfiedBy(applicant) ? 1 : 0
            };

        private double ScaledSuitability(bool in_range, bool scale_suitability, uint mark, uint max_mark)
        {
            if (!in_range)
                return 0;
            else if (scale_suitability)
                return mark / max_mark;
            else
                return 1;
        }

        /// <summary>Dummy value counts top level AbilityExact/AbilityRange requirements only</summary>
        public double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role)
            => applicant.Roles.Where(r => r != excluding_role)
            .Where(r => r.Requirements.Any(req => req is ICriteriaRequirement cr && cr.Criteria == criteria))
            .Count();

        /// <summary>Dummy value is always available unless they are already cast in a role in any of the items this role is in, except this role</summary>
        public Availability AvailabilityOf(Applicant applicant, Role role)
            => role.Items.SelectMany(i => i.Roles).Distinct()
            .Where(r => r != role)
            .Any(r => r.Cast.Contains(applicant))
            ? Availability.AlreadyInItem : Availability.Available;

        /// <summary>Dummy value enumerates roles in item order, then by name, removing duplicates</summary>
        public IEnumerable<Role> CastingOrder(IEnumerable<Item> items_in_order)
            => items_in_order.SelectMany(i => i.Roles.OrderBy(r => r.Name)).Distinct();

        /// <summary>
        /// Dummy selection picks the number of required applicants of each type from the list of applicants, ignoring availability
        /// </summary>
        public IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts)
        {
            var casts = alternative_casts.ToArray();
            foreach (var cbg in role.CountByGroups)
            {
                if (cbg.CastGroup.AlternateCasts)
                {
                    foreach(var cast in casts)
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

        //TODO this should be in AllocateCastNumbers
        //int num = 1;
        //foreach (var cast_group in cast_groups)
        //{
        //    var group_casts = cast_group.AlternateCasts ? alternative_casts : new AlternativeCast?[] { null };
        //    int ac = 0;
        //    foreach (var applicant in cast_group.Members)
        //    {
        //        applicant.CastNumber = num;
        //        applicant.AlternativeCast = group_casts[ac++];
        //        if (ac >= group_casts.Length)
        //        {
        //            ac = 0;
        //            num++;
        //        }
        //    }
        //}

        //TODO implement dummy engine
        public void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups) => throw new NotImplementedException();
        public void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<AlternativeCast> alternative_casts, IEnumerable<SameCastSet> same_cast_sets) => throw new NotImplementedException();
        public void AllocateCastNumbers(IEnumerable<Applicant> applicants, Criteria order_by, ListSortDirection sort_direction = ListSortDirection.Ascending) => throw new NotImplementedException();
        public void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags) => throw new NotImplementedException();
        public int OverallAbility(Applicant applicant) => throw new NotImplementedException();
        public IEnumerable<Role> IdealCastingOrder(IEnumerable<Item> items_in_order) => throw new NotImplementedException();
    }
}

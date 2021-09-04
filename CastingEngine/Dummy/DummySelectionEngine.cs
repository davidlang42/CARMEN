using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Selection;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.CastingEngine.Dummy
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummySelectionEngine : SelectionEngine
    {
        public DummySelectionEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        { }

        /// <summary>Dummy implementation selects the required number of cast in each group, in random order, overwriting every applicants existing cast group</summary>
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
        {
            var e = applicants.GetEnumerator();
            foreach (var cast_group in cast_groups)
            {
                var required = cast_group.RequiredCount ?? 0;
                if (cast_group.AlternateCasts)
                    required *= (uint)alternativeCasts.Length;
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
        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets)
        {
            var cast_groups = applicants.Select(a => a.CastGroup).OfType<CastGroup>().ToHashSet();
            foreach (var cast_group in cast_groups)
            {
                var group_casts = cast_group.AlternateCasts ? alternativeCasts : new AlternativeCast?[] { null };
                int ac = 0;
                foreach (var applicant in cast_group.Members)
                {
                    applicant.AlternativeCast = group_casts[ac++];
                    if (ac >= group_casts.Length)
                        ac = 0;
                }
            }
        }

        /// <summary>Dummy implementation allocates cast numbers for each cast group in order, in a random order</summary>
        public override void AllocateCastNumbers(IEnumerable<Applicant> applicants)
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
        public override void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                tag.Members.Clear();
                foreach (var cbg in tag.CountByGroups)
                {
                    var required = (int)cbg.Count;
                    if (cbg.CastGroup.AlternateCasts)
                        required *= alternativeCasts.Length;
                    foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup).Take(required))
                        tag.Members.Add(applicant);
                }
            }
        }
    }
}

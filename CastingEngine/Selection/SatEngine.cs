using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// The base class of all SAT based selection engines
    /// </summary>
    public abstract class SatEngine : SelectionEngine
    {
        protected Criteria[] primaryCriterias;

        // expression building is O(2^n) and will be slow for large numbers, therefore cache
        Dictionary<int, ExpressionBuilder> cachedKeepTogether = new();

        public SatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        {
            primaryCriterias = criterias.Where(c => c.Primary).ToArray();
        }

        /// <summary>Select applicants into Cast Groups, respecting those already selected, by calculating the remaining count for each group,
        /// then taking the applicants with the highest overall ability until all cast groups are filled or there are no more applicants.
        /// NOTE: This currently ignores the suitability of the applicant for the Cast Group, only checking that the minimum requirements are satisfied,
        /// but this should be improved in the future.</summary>
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
        {
            //LATER add a comment about if an applicant meets the requirements for multiple cast groups, it chooses the first cast group in order which has an available spot (and implement that)
            // also make these changes back into the heuristic engine, or move this to be a common implmenetation, possibly tweak so that it chooses the cast group which an applicant is most suitable for, but think about the use case
            //LATER handle the fact that cast group requirements may not be mutually exclusive, possibly using SAT (current implementation is copied from HeuristicSelectionEngine)
            //LATER use SuitabilityOf(Applicant, CastGroup) to order applicants, handling ties with OverallAbility
            // In this dictionary, a value of null means infinite are allowed, but if the key is missing that means no more are allowed
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            // Calculate the remaining number of cast needed in each group (respecting those already accepted)
            foreach (var cast_group in cast_groups)
            {
                // Check that requirements don't include TagRequirements
                var tag_requirements = cast_group.Requirements
                    .SelectMany(r => r.References()).Concat(cast_group.Requirements)
                    .OfType<TagRequirement>();
                if (tag_requirements.Any())
                    throw new ApplicationException("Cast group requirements cannot refer to Tags.");
                // Add remaining cast to dictionary
                if (cast_group.RequiredCount is uint required)
                {
                    if (cast_group.AlternateCasts)
                        required *= (uint)alternativeCasts.Length;
                    required -= (uint)cast_group.Members.Count;
                    if (required > 0)
                        remaining_groups.Add(cast_group, required);
                }
                else
                    remaining_groups.Add(cast_group, null);
            }
            // Allocate non-accepted applicants to cast groups, until the remaining counts are 0
            foreach (var applicant in applicants.Where(a => !a.IsAccepted).OrderByDescending(a => AuditionEngine.OverallAbility(a)))
            {
                if (NextAvailableCastGroup(remaining_groups, applicant) is CastGroup cg)
                {
                    applicant.CastGroup = cg;
                    if (remaining_groups[cg] is uint remaining_count)
                    {
                        if (remaining_count == 1)
                            remaining_groups.Remove(cg);
                        else
                            remaining_groups[cg] = remaining_count - 1;
                    }
                }
                if (remaining_groups.Count == 0)
                    break;
            }
        }

        /// <summary>Find a cast group with availability for which this applicant meets the cast group's requirements</summary>
        private CastGroup? NextAvailableCastGroup(Dictionary<CastGroup, uint?> remaining_groups, Applicant applicant)
        {
            foreach (var (cg, _) in remaining_groups)//LATER implement order here, can probably use .Keys
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }

        protected abstract Solver<Applicant> BuildSatSolver(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast);

        protected abstract Solution FindSatSolution(Solver<Applicant> sat, List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast,
            List<Clause<Applicant>> existing_assignments, List<Clause<Applicant>> same_cast_clauses, Dictionary<Applicant, SameCastSet> same_cast_lookup);

        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets)
        {
            if (alternativeCasts.Length != 2)
                throw new ApplicationException("This approach only works for exactly 2 alternative casts.");
            // sort the applicants into cast groups
            var applicants_needing_alternative_cast = new List<(CastGroup, HashSet<Applicant>)>();
            var existing_assignments = new List<Clause<Applicant>>();
            foreach (var applicants_group in applicants.GroupBy(a => a.CastGroup))
            {
                if (applicants_group.Key is not CastGroup cast_group // not accepted into cast
                    || !cast_group.AlternateCasts) // non-alternating cast group
                {
                    // alternative cast not required, set to null
                    foreach (var applicant in applicants_group)
                        applicant.AlternativeCast = null;
                }
                else
                {
                    var applicants_set = applicants_group.ToHashSet();
                    applicants_needing_alternative_cast.Add((cast_group, applicants_set));
                    // add clauses for applicants with alternative cast already set
                    foreach (var applicant in applicants_set)
                        if (applicant.AlternativeCast != null)
                            existing_assignments.Add(Clause<Applicant>.Unit(applicant, applicant.AlternativeCast == alternativeCasts[1]));
                }
            }
            if (applicants_needing_alternative_cast.Count == 0)
                return; // nothing to do
            // create clauses for same cast sets
            var same_cast_clauses = new List<Clause<Applicant>>();
            var same_cast_lookup = new Dictionary<Applicant, SameCastSet>();
            foreach (var same_cast_set in same_cast_sets)
            {
                foreach (var applicant in same_cast_set.Applicants)
                    same_cast_lookup.Add(applicant, same_cast_set); // Applicants can only be in one SameCastSet
                same_cast_clauses.AddRange(KeepTogether(same_cast_set.Applicants.Where(a => applicants_needing_alternative_cast.Any(p => p.Item2.Contains(a))).ToArray()));
            }
            // run approach-specific SAT solving
            var sat = BuildSatSolver(applicants_needing_alternative_cast);
            var solution = FindSatSolution(sat, applicants_needing_alternative_cast, existing_assignments, same_cast_clauses, same_cast_lookup);
            // set alternative casts
            List<IEnumerable<Assignment<Applicant>>> grouped_assignments;
            if (!solution.IsUnsolvable)
                // assigned by sat solver
                grouped_assignments = sat.MapAssignments(solution)
                    .GroupBy(a => a.Variable.CastGroup)
                    .Select(g => g.AsEnumerable()).ToList();
            else
                // default to applicants evenly filled
                grouped_assignments = applicants_needing_alternative_cast
                    .Select(p => p.Item2.Select(a => new Assignment<Applicant>
                    {
                        Variable = a,
                        Value = a.AlternativeCast?.Equals(alternativeCasts[1]) // keep the existing value if set
                    })).ToList();
            foreach (var group in grouped_assignments)
            {
                var full_assignments = EvenlyFillAssignments(group);
                foreach (var assignment in full_assignments)
                    assignment.Variable.AlternativeCast = alternativeCasts[assignment.Value!.Value ? 1 : 0];
            }
        }

        /// <summary>Creates a clause specifying that all these applicants are in the same alternative cast</summary>
        private IEnumerable<Clause<Applicant>> KeepTogether(Applicant[] applicants)
        {
            if (!cachedKeepTogether.TryGetValue(applicants.Length, out var builder))
            {
                builder = new ExpressionBuilder(applicants.Length, values => values.All(v => v) || values.All(v => !v));
                cachedKeepTogether.Add(applicants.Length, builder);
            }
            foreach (var clause in builder.Apply(applicants).Clauses)
                yield return clause;
        }

        /// <summary>Fills any free (null) assignments, keeping the totals in each alternative cast as close to equal as possible</summary>
        private Assignment<Applicant>[] EvenlyFillAssignments(IEnumerable<Assignment<Applicant>> assignments)
        {
            var result = assignments.OrderByDescending(a => AuditionEngine.OverallAbility(a.Variable)).ToArray();
            int count_true = 0;
            int count_false = 0;
            for (var r = 0; r < result.Length; r++)
                if (result[r].Value is bool value)
                {
                    if (value)
                        count_true++;
                    else
                        count_false++;
                }
            if (count_true + count_false != result.Length) // don't reiterate if no gaps
                for (var r = 0; r < result.Length; r++)
                    if (!result[r].Value.HasValue)
                        if (count_true < count_false)
                        {
                            result[r].Value = true;
                            count_true++;
                        }
                        else
                        {
                            result[r].Value = false;
                            count_false++;
                        }
            return result;
        }
    }
}

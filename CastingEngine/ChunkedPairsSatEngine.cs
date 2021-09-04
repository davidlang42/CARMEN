using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    public class ChunkedPairsSatEngine : WeightedSumEngine, ISelectionEngine
    {
        // expression building is O(2^n) and will be slow for large numbers, therefore cache
        Dictionary<int, ExpressionBuilder> cachedKeepTogether = new();
        Dictionary<int, ExpressionBuilder> cachedKeepSeparate = new();

        public List<Result> Results { get; init; } = new(); //LATER read only

        public struct Result
        {
            public int ChunkSize;
            public int Variables;
            public int Clauses;
            public int MaxLiterals;
            public bool Solved;
        }

        #region Copied from OriginalHeuristicEngine
        public void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts) //LATER copied from OriginalHeuristicEngine
        {
            // In this dictionary, a value of null means infinite are allowed, but if the key is missing that means no more are allowed
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            // Calculate the remaining number of cast needed in each group (respecting those already accepted)
            foreach (var cast_group in cast_groups)
            {
                if (cast_group.RequiredCount is uint required)
                {
                    if (cast_group.AlternateCasts)
                        required *= number_of_alternative_casts;
                    required -= (uint)cast_group.Members.Count;
                    if (required > 0)
                        remaining_groups.Add(cast_group, required);
                }
                else
                    remaining_groups.Add(cast_group, null);
            }
            // Allocate non-accepted applicants to cast groups, until the remaining counts are 0
            foreach (var applicant in applicants.Where(a => !a.IsAccepted).OrderByDescending(a => OverallAbility(a)))
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
        private CastGroup? NextAvailableCastGroup(Dictionary<CastGroup, uint?> remaining_groups, Applicant applicant) //LATER copied from OriginalHeuristicEngine
        {
            foreach (var (cg, remaining_count) in remaining_groups)
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }

        /// <summary>Orders the applicants by the specified criteria</summary>
        private IEnumerable<Applicant> Sort(IEnumerable<Applicant> applicants, Criteria? by, ListSortDirection direction)
            => (by, direction) switch
            {
                (Criteria c, ListSortDirection.Ascending) => applicants.OrderBy(a => a.MarkFor(c)),
                (Criteria c, ListSortDirection.Descending) => applicants.OrderByDescending(a => a.MarkFor(c)),
                (null, ListSortDirection.Ascending) => applicants.OrderBy(a => OverallAbility(a)),
                (null, ListSortDirection.Descending) => applicants.OrderByDescending(a => OverallAbility(a)),
                _ => throw new ApplicationException($"Sort not handled: {by} / {direction}")
            };

        public void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction)
        {
            var cast_numbers = new CastNumberSet();
            // find cast numbers which are already set
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup != null)
                {
                    if (applicant.CastNumber is int cast_number)
                        if (!cast_numbers.Add(cast_number, applicant.AlternativeCast, applicant.CastGroup))
                            // if add fails, this cast number has already been allocated, therefore remove it
                            applicant.CastNumber = null;
                }
                else
                {
                    // clear cast numbers of rejected applicants
                    applicant.CastNumber = null;
                }
            }
            // allocate cast numbers to those who need them
            foreach (var applicant in Sort(applicants.Where(a => a.IsAccepted), order_by, sort_direction))
            {
                if (applicant.CastNumber == null)
                    applicant.CastNumber = cast_numbers.AddNextAvailable(applicant.AlternativeCast, applicant.CastGroup!); // not null because IsAccepted
            }
        }

        public void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts)
        {
            // allocate tags sequentially because they aren't dependant on each other
            tags = tags.ToArray(); //TODO (HEURISTIC) fix this hack for concurrent modification of collecion
            foreach (var tag in tags)
                ApplyTag(applicants, tag, number_of_alternative_casts);
        }

        public void ApplyTag(IEnumerable<Applicant> applicants, Tag tag, uint number_of_alternative_casts)
        {
            // In this dictionary, if a key is missing that means infinite are allowed
            var remaining = tag.CountByGroups.ToDictionary(
                cbg => cbg.CastGroup,
                cbg => cbg.CastGroup.AlternateCasts ? number_of_alternative_casts * cbg.Count : cbg.Count);
            // Subtract cast already allocated to tags
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup == null)
                    applicant.Tags.Remove(tag); // not accepted, therefore remove tag
                else if (applicant.Tags.Contains(tag)
                    && remaining.TryGetValue(applicant.CastGroup, out var remaining_count)
                    && remaining_count != 0)
                    remaining[applicant.CastGroup] = remaining_count - 1;
            }
            // Apply tags to accepted applicants in order of suitability
            foreach (var applicant in applicants.Where(a => a.IsAccepted).OrderByDescending(a => SuitabilityOf(a, tag.Requirements))) //TODO (HEURISTIC) ModifiedHeuristicEngine should fallback to OverallAbility if tied on tag requirements
            {
                if (tag.Requirements.All(r => r.IsSatisfiedBy(applicant))) // cast member meets minimum requirement
                {
                    var cast_group = applicant.CastGroup!; // not null because IsAccepted
                    if (!remaining.TryGetValue(cast_group, out var remaining_count))
                        applicant.Tags.Add(tag); // no limit on this cast group
                    else if (remaining_count > 0)
                    {
                        // limited, but space remaining for this cast group
                        applicant.Tags.Add(tag);
                        remaining[cast_group] = remaining_count - 1;
                    }
                }
            }
        }

        /// <summary>Calculate the suitability of an applicant against a set of requirements.
        /// This assumes no circular references.</summary>
        private double SuitabilityOf(Applicant applicant, IEnumerable<Requirement> requirements)
        {
            var sub_suitabilities = requirements.Select(req => SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
        }

        private double SuitabilityOf(Applicant applicant, Requirement requirement) //LATER copied from DummyEngine
            => requirement switch
            {
                AbilityRangeRequirement arr => ScaledSuitability(arr.IsSatisfiedBy(applicant), arr.ScaleSuitability, applicant.MarkFor(arr.Criteria), arr.Criteria.MaxMark),
                NotRequirement nr => 1 - SuitabilityOf(applicant, nr.SubRequirement),
                AndRequirement ar => ar.AverageSuitability ? ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Product(), //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
                OrRequirement or => or.AverageSuitability ? or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Max(), //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
                XorRequirement xr => xr.SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe() is Requirement req ? SuitabilityOf(applicant, req) : 0,
                Requirement req => req.IsSatisfiedBy(applicant) ? 1 : 0
            };

        private double ScaledSuitability(bool in_range, bool scale_suitability, uint mark, uint max_mark) //LATER copied from DummyEngine
        {
            //LATER real implementation might not test if in range
            if (!in_range)
                return 0;
            else if (scale_suitability)
                return mark / max_mark;
            else
                return 1;
        }
        #endregion

        private Criteria[] criterias;

        public ChunkedPairsSatEngine(Criteria[] criterias)
            : base(criterias)
        {
            this.criterias = criterias;
        }

        public void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets)
        {
            if (alternative_casts.Length != 2)
                throw new ApplicationException("The Chunked-Pairs approach only works for exactly 2 alternative casts.");
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
                            existing_assignments.Add(Clause<Applicant>.Unit(applicant, applicant.AlternativeCast == alternative_casts[1]));
                }
            }
            // create clauses for same cast sets
            var same_cast_clauses = new List<Clause<Applicant>>();
            var same_cast_lookup = new Dictionary<Applicant, SameCastSet>();
            foreach (var same_cast_set in same_cast_sets)
            {
                foreach (var applicant in same_cast_set)
                    same_cast_lookup.Add(applicant, same_cast_set); // Applicants can only be in one SameCastSet
                same_cast_clauses.AddRange(KeepTogether(same_cast_set.Where(a => applicants_needing_alternative_cast.Any(p => p.Item2.Contains(a))).ToArray()));
            }
            // start with pairs, then increase chunk size until success
            var sat = new DpllSolver<Applicant>(applicants_needing_alternative_cast.SelectMany(p => p.Item2));
            var max_chunk = applicants_needing_alternative_cast.Max(p => p.Item2.Count);
            Solution solution = Solution.Unsolveable;
            Results.Clear();
            for (int chunk_size = 2; chunk_size <= max_chunk; chunk_size += 2)
            {
                // compile clauses
                var clauses = new HashSet<Clause<Applicant>>();
                clauses.AddRange(existing_assignments);
                clauses.AddRange(same_cast_clauses);
                foreach (var (cg, hs) in applicants_needing_alternative_cast)
                    clauses.AddRange(BuildChunkClauses(hs, chunk_size, same_cast_lookup));
                if (clauses.Count == 0)
                    break; // no clauses to solve
                // run sat solver
                solution = sat.Solve(new() { Clauses = clauses }).FirstOrDefault();
                Results.Add(new Result
                {
                    ChunkSize = chunk_size,
                    Variables = sat.Variables.Count,
                    Clauses = clauses.Count,
                    MaxLiterals = clauses.Max(c => c.Literals.Count),
                    Solved = !solution.IsUnsolvable
                });
                if (!solution.IsUnsolvable)
                    break; // solved
            }
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
                    .Select(p => p.Item2.Select(a => new Assignment<Applicant> {
                        Variable = a,
                        Value = a.AlternativeCast?.Equals(alternative_casts[1]) // keep the existing value if set
                    })).ToList();
            foreach (var group in grouped_assignments)
            {
                var full_assignments = EvenlyFillAssignments(group);
                foreach (var assignment in full_assignments)
                    assignment.Variable.AlternativeCast = alternative_casts[assignment.Value!.Value ? 1 : 0];
            }
        }

        /// <summary>Creates clauses for chunks of applicants within one cast group, for each primary criteria</summary>
        private IEnumerable<Clause<Applicant>> BuildChunkClauses(IEnumerable<Applicant> applicants, int chunk_size, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            foreach (var criteria in criterias.Where(c => c.Primary))
            {
                var sorted_applicants = new Queue<Applicant>(applicants.OrderByDescending(a => a.MarkFor(criteria)));
                while (sorted_applicants.Count >= chunk_size)
                {
                    // TakeChunk may still fail, even though there are technically enough applicants,
                    // if the remaining applicants are in a SameCastSet
                    if (TakeChunk(sorted_applicants, chunk_size, same_cast_lookup) is Applicant[] chunk)
                        foreach (var clause in KeepSeparate(chunk))
                            yield return clause;
                }
            }
        }

        /// <summary>Creates a clause specifying that all these applicants are in the same alternative cast</summary>
        private IEnumerable<Clause<Applicant>> KeepTogether(Applicant[] applicants)
        {
            //LATER cache builders, because they are probably quite slow (cache should persist at least as long as the engine, maybe application, maybe to disk)
            var builder = new ExpressionBuilder(applicants.Length, values => values.All(v => v) || values.All(v => !v));
            foreach (var clause in builder.Apply(applicants).Clauses)
                yield return clause;
        }

        /// <summary>Creates a clause specifying that exactly half of the given applicants are in each alternative cast</summary>
        private IEnumerable<Clause<Applicant>> KeepSeparate(Applicant[] applicants)
        {
            //LATER cache builders, because they are probably quite slow (cache should persist at least as long as the engine, maybe application, maybe to disk)
            var builder = new ExpressionBuilder(applicants.Length, values => values.Count(v => v) == applicants.Length / 2);
            foreach (var clause in builder.Apply(applicants).Clauses)
                yield return clause;
        }

        /// <summary>Dequeues chunk_size applicants from the start of the queue, with at most chunk_size/2 from any one SameCastSet</summary>
        private Applicant[]? TakeChunk(Queue<Applicant> applicants, int chunk_size, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            var chunk = new Applicant[chunk_size];
            var c = 0;
            var skipped = new Queue<Applicant>();
            while (c < chunk.Length)
            {
                if (!applicants.TryDequeue(out var next_applicant))
                    break;
                if (same_cast_lookup.TryGetValue(next_applicant, out var same_cast_set)
                    && chunk.Count(c => same_cast_set.Contains(c)) + 1 > chunk_size / 2)
                    skipped.Prepend(next_applicant);
                else
                    chunk[c++] = next_applicant;
            }
            foreach (var skip in skipped)
                applicants.Prepend(skip);
            if (c == chunk.Length)
                return chunk;
            else
                return null; // incomplete chunk
        }

        /// <summary>Fills any free (null) assignments, keeping the totals in each alternative cast as close to equal as possible</summary>
        private Assignment<Applicant>[] EvenlyFillAssignments(IEnumerable<Assignment<Applicant>> assignments)
        {
            var result = assignments.OrderByDescending(a => OverallAbility(a.Variable)).ToArray();
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

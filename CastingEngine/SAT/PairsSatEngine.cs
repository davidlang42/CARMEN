using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.SAT;
using Carmen.CastingEngine.SAT.Internal;
using Carmen.CastingEngine.Selection;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A type of approach for balancing alternative casts which uses a DPLL solver for the k-SAT boolean satisfiability problem,
    /// modelling each Applicant's choice between 2 alternative casts as a boolean variable.
    /// NOTE: This only works for exactly 2 alternative casts
    /// </summary>
    public abstract class PairsSatEngine : SatEngine
    {
        public List<Result> Results { get; init; } = new(); //LATER read only

        public struct Result
        {
            public int ChunkSize;
            public int MaxChunks;
            public int Variables;
            public int Clauses;
            public int MaxLiterals;
            public bool Solved;
        }

        private Criteria[] primaryCriterias;

        // expression building is O(2^n) and will be slow for large numbers, therefore cache
        Dictionary<int, ExpressionBuilder> cachedKeepTogether = new();
        Dictionary<int, ExpressionBuilder> cachedKeepSeparate = new();

        public PairsSatEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        {
            primaryCriterias = criterias.Where(c => c.Primary).ToArray();
        }

        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets)
        {
            if (alternativeCasts.Length != 2)
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
                            existing_assignments.Add(Clause<Applicant>.Unit(applicant, applicant.AlternativeCast == alternativeCasts[1]));
                }
            }
            // create clauses for same cast sets
            var same_cast_clauses = new List<Clause<Applicant>>();
            var same_cast_lookup = new Dictionary<Applicant, SameCastSet>();
            foreach (var same_cast_set in same_cast_sets)
            {
                foreach (var applicant in same_cast_set.Applicants)
                    same_cast_lookup.Add(applicant, same_cast_set); // Applicants can only be in one SameCastSet
                same_cast_clauses.AddRange(KeepTogether(same_cast_set.Applicants.Where(a => applicants_needing_alternative_cast.Any(p => p.Item2.Contains(a))).ToArray()));
            }
            // Initialise() the starting conditions, then Simplify() until the SAT solver succeeds
            var sat = new DpllSolver<Applicant>(applicants_needing_alternative_cast.SelectMany(p => p.Item2));
            Solution solution = Solution.Unsolveable;
            Results.Clear();
            Initialise(applicants_needing_alternative_cast.Select(p => p.Item2.Count), out int chunk_size, out int max_chunks);
            do //LATER probably need some way of communicating progress back to the main program, especially as chunk size increases in ChunkedPairsSatEngine
            {
                // compile clauses
                var clauses = new HashSet<Clause<Applicant>>();
                clauses.AddRange(existing_assignments);
                clauses.AddRange(same_cast_clauses);
                bool any_chunk_clause = false;
                foreach (var (cg, hs) in applicants_needing_alternative_cast)
                    any_chunk_clause |= clauses.AddRange(BuildChunkClauses(hs, chunk_size, max_chunks, same_cast_lookup));
                if (clauses.Count == 0)
                    break; // no clauses to solve
                // run sat solver
                solution = sat.Solve(new() { Clauses = clauses }).FirstOrDefault();
                Results.Add(new Result
                {
                    ChunkSize = chunk_size,
                    MaxChunks = max_chunks,
                    Variables = sat.Variables.Count,
                    Clauses = clauses.Count,
                    MaxLiterals = clauses.Max(c => c.Literals.Count),
                    Solved = !solution.IsUnsolvable
                });
                if (!solution.IsUnsolvable)
                    break; // solved
                if (!any_chunk_clause)
                    break; // we didn't have any chunk clauses, if this didn't solve, nothing will
            } while (Simplify(ref chunk_size, ref max_chunks));
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

        /// <summary>This is called to initialise the chunking parameters before we start SAT solving.
        /// The default implementation is to start by chunking the whole list of applicants into pairs.
        /// It may be helpful to override this if your Simplify() method uses state which needs to be
        /// reset between runs.</summary>
        protected virtual void Initialise(IEnumerable<int> applicants_per_cast_group, out int chunk_size, out int max_chunks)
        {
            chunk_size = 2;
            max_chunks = applicants_per_cast_group.Max() / chunk_size; // at this value, the limit will not be hit before all applicants have been chunked
        }

        /// <summary>If the current SAT problem is unsolvable, this is called to modify the chunking parameters to make
        /// it a simplier SAT problem to solve. If there is no simplier option, return false to stop trying.
        /// As a failsafe, if the chunking parameters fail to create any chunks, iteration will terminate.</summary>
        protected abstract bool Simplify(ref int chunk_size, ref int max_chunks);

        /// <summary>Creates clauses for chunks of applicants within one cast group, for each primary criteria</summary>
        private IEnumerable<Clause<Applicant>> BuildChunkClauses(IEnumerable<Applicant> applicants, int chunk_size, int max_chunks, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            foreach (var criteria in primaryCriterias)
            {
                int chunk_count = 0;
                var sorted_applicants = new Stack<Applicant>(applicants.OrderBy(a => a.MarkFor(criteria))); // order by marks ascending so that the lowest mark is at the bottom of the stack
                while (sorted_applicants.Count >= chunk_size && chunk_count < max_chunks)
                {
                    if (TakeChunk(sorted_applicants, chunk_size, same_cast_lookup) is Applicant[] chunk)
                        foreach (var clause in KeepSeparate(chunk))
                            yield return clause;
                    else
                        // TakeChunk may fail, even though there are technically enough applicants,
                        // if the remaining applicants are in a SameCastSet
                        break;
                    chunk_count++;
                }
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

        /// <summary>Creates a clause specifying that exactly half of the given applicants are in each alternative cast</summary>
        private IEnumerable<Clause<Applicant>> KeepSeparate(Applicant[] applicants)
        {
            if (!cachedKeepSeparate.TryGetValue(applicants.Length, out var builder))
            {
                builder = new ExpressionBuilder(applicants.Length, values => values.Count(v => v) == applicants.Length / 2);
                cachedKeepSeparate.Add(applicants.Length, builder);
            }
            foreach (var clause in builder.Apply(applicants).Clauses)
                yield return clause;
        }

        /// <summary>Takes chunk_size applicants from the top of the stack, with at most chunk_size/2 from any one SameCastSet</summary>
        private Applicant[]? TakeChunk(Stack<Applicant> applicants, int chunk_size, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            var chunk = new Applicant[chunk_size];
            var c = 0;
            var skipped = new Stack<Applicant>();
            while (c < chunk.Length)
            {
                if (!applicants.TryPop(out var next_applicant))
                    break;
                if (same_cast_lookup.TryGetValue(next_applicant, out var same_cast_set)
                    && chunk.Count(c => same_cast_set.Applicants.Contains(c)) + 1 > chunk_size / 2)
                    skipped.Push(next_applicant);
                else
                    chunk[c++] = next_applicant;
            }
            while (skipped.Count > 0)
                applicants.Push(skipped.Pop());
            if (c == chunk.Length)
                return chunk;
            else
                return null; // incomplete chunk
        }

        /// <summary>Fills any free (null) assignments, keeping the totals in each alternative cast as close to equal as possible</summary>
        private Assignment<Applicant>[] EvenlyFillAssignments(IEnumerable<Assignment<Applicant>> assignments)
        {
            var result = assignments.OrderByDescending(a => ApplicantEngine.OverallAbility(a.Variable)).ToArray();
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

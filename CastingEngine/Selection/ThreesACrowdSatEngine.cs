using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A concrete approach for balancing alternative casts which uses a DPLL(T) solver for the SMT satisfiability problem,
    /// by creating overlapping sets of 3 applicants which must not all be in the same cast. The total number of applicants
    /// in each cast is ensured by the validity theory test. If unsolvable, the number of sets is reduced until it is.
    /// NOTE: This currently only works for exactly 2 alternative casts
    /// </summary>
    public class ThreesACrowdSatEngine : SatEngine
    {
        public List<Result> Results { get; init; } = new();

        public struct Result
        {
            public int SetSize;
            public int MaxSets;
            public int Variables;
            public int Clauses;
            public int MaxLiterals;
            public bool Solved;
        }

        public ThreesACrowdSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        /// <summary>Verifies that the number of cast assigned to each alternative cast is the same for all cast groups.
        /// If the number of cast in a cast group is odd, the spare cast member can be either alternative cast.</summary>
        private bool? AlternativeCastsAreEqual(bool?[] assignments, CastGroup[] cast_groups)
        {
            if (assignments.Length != cast_groups.Length)
                throw new ArgumentException("Assignments and cast groups must have the same number of elements");
            var count_true = new OmnificentDictionary<CastGroup, int>();
            var count_false = new OmnificentDictionary<CastGroup, int>();
            var count_total = new OmnificentDictionary<CastGroup, int>();
            for (var i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] == true)
                    count_true[cast_groups[i]] += 1;
                else if (assignments[i] == false)
                    count_false[cast_groups[i]] += 1;
                count_total[cast_groups[i]] += 1;
            }
            foreach (var (cg, total) in count_total)
            {
                int max = (total + 1) / 2;
                if (count_true[cg] > max || count_false[cg] > max)
                    return false;
            }
            foreach (var (cg, total) in count_total)
            {
                int min = total / 2;
                if (count_true[cg] < min || count_false[cg] < min)
                    return null;
            }
            return true;
        }

        protected override Solver<Applicant> BuildSatSolver(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast)
        {
            var all_applicants = applicants_needing_alternative_cast.SelectMany(p => p.Item2).ToArray();
            var applicant_cast_groups = all_applicants.Select(a => a.CastGroup!).ToArray();
            return new DpllTheorySolver<Applicant>(soln => AlternativeCastsAreEqual(soln.Assignments, applicant_cast_groups), all_applicants);
        }

        protected override Solution FindSatSolution(Solver<Applicant> sat, List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast,
            List<Clause<Applicant>> existing_assignments, List<Clause<Applicant>> same_cast_clauses, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            Solution solution = Solution.Unsolveable;
            // Initialise() the starting conditions, then Simplify() until the SAT solver succeeds
            Results.Clear();
            Initialise(applicants_needing_alternative_cast.Select(p => p.Item2.Count), out int set_size, out int max_sets);
            do
            {
                // compile clauses
                var clauses = new HashSet<Clause<Applicant>>();
                clauses.AddRange(existing_assignments);
                clauses.AddRange(same_cast_clauses);
                bool any_set_clauses = false;
                foreach (var (cg, hs) in applicants_needing_alternative_cast)
                    any_set_clauses |= clauses.AddRange(BuildOverlappingSetClauses(hs, set_size, max_sets, same_cast_lookup));
                if (clauses.Count == 0)
                    break; // no clauses to solve
                // run sat solver
                solution = sat.Solve(new(clauses)).FirstOrDefault();
                Results.Add(new Result
                {
                    SetSize = set_size,
                    MaxSets = max_sets,
                    Variables = sat.Variables.Count,
                    Clauses = clauses.Count,
                    MaxLiterals = clauses.Max(c => c.Literals.Count),
                    Solved = !solution.IsUnsolvable
                });
                if (!solution.IsUnsolvable)
                    break; // solved
                if (!any_set_clauses)
                    break; // we didn't have any sets clauses, if this didn't solve, nothing will
            } while (Simplify(ref set_size, ref max_sets));
            return solution;
        }

        /// <summary>This is called to initialise the overlapping set parameters before we start SAT solving.
        /// The default implementation is to start by making overlapping sets of 3 across the whole list of applicants.
        /// It may be helpful to override this if your Simplify() method uses state which needs to be
        /// reset between runs.</summary>
        protected virtual void Initialise(IEnumerable<int> applicants_per_cast_group, out int set_size, out int max_sets)
        {
            set_size = 3;
            max_sets = applicants_per_cast_group.Max() - set_size + 1; // at this value, the limit will not be hit before all applicants have been placed in sets
        }

        /// <summary>If the current SAT problem is unsolvable, this is called to modify the overlapping set parameters to make
        /// it a simplier SAT problem to solve. If there is no simplier option, return false to stop trying.
        /// As a failsafe, if the set parameters fail to create any sets, iteration will terminate.
        /// The default implementation reduces the number of sets by 1 each iteration.</summary>
        protected virtual bool Simplify(ref int set_size, ref int max_sets)
        {
            max_sets -= 1;
            return max_sets >= 0; // allow zero in case the basic clauses are solvable on their own
        }

        /// <summary>Creates clauses for overlapped sets of applicants, within one cast group, for each primary criteria</summary>
        private IEnumerable<Clause<Applicant>> BuildOverlappingSetClauses(IEnumerable<Applicant> applicants, int set_size, int max_sets, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            foreach (var criteria in primaryCriterias)
            {
                int set_count = 0;
                var sorted_applicants = applicants.OrderByDescending(a => a.MarkFor(criteria)).ToArray();
                for (var i = 0; i < sorted_applicants.Length - set_size + 1; i++)
                {
                    if (TakeSet(sorted_applicants, i, set_size, same_cast_lookup) is Applicant[] set)
                        foreach (var clause in KeepNotAllEqual(set))
                            yield return clause;
                    else
                        // TakeSet may fail if the remaining applicants are in a SameCastSet
                        break;
                    set_count++;
                    if (set_count == max_sets)
                        break;
                }
            }
        }

        /// <summary>Creates a clause specifying that not all of the given applicants are in the same alternative cast</summary>
        private IEnumerable<Clause<Applicant>> KeepNotAllEqual(Applicant[] applicants)
        {
            // (A,B,C) is not all equal if (A || B || C) && (A' || B' || C')
            yield return new(applicants.Select(a => Literal<Applicant>.Positive(a)).ToHashSet());
            yield return new(applicants.Select(a => Literal<Applicant>.Negative(a)).ToHashSet());
        }

        /// <summary>Takes set_size applicants starting at start_index, with at most set_size-1 from any one SameCastSet</summary>
        private Applicant[]? TakeSet(Applicant[] applicants, int start_index, int set_size, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            var set = new Applicant[set_size];
            var s = 0;
            for (var i=start_index; i < applicants.Length; i++)
            {
                var next_applicant = applicants[i];
                if (!(same_cast_lookup.TryGetValue(next_applicant, out var same_cast_set)
                    && set.Count(a => same_cast_set.Applicants.Contains(a)) + 1 > set_size - 1))
                    set[s++] = next_applicant;
                if (s == set.Length)
                    return set;
            }
            return null; // incomplete set
        }
    }
}

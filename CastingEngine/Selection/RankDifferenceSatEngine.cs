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
    /// A concrete approach for balancing alternative casts by minimising difference of the sum of ranks
    /// for each criteria, between casts.
    /// NOTE: This only works for exactly 2 alternative casts
    /// </summary>
    public class RankDifferenceSatEngine : SatEngine
    {
        public const int RANK_INCREMENT = 2;
        public const int TIMEOUT_MS = 5000;

        int[][] applicantRanks = Array.Empty<int[]>(); // [criteria][applicant variable]
        Applicant[] applicantVariables = Array.Empty<Applicant>();
        int[] castGroupIndexFromVariableIndex = Array.Empty<int>();
        CastGroup[] castGroups = Array.Empty<CastGroup>();

        public RankDifferenceSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override Solver<Applicant> BuildSatSolver(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast)
        {
            CalculateRankings(primaryCriterias, applicants_needing_alternative_cast, out applicantVariables, out castGroups, out applicantRanks, out castGroupIndexFromVariableIndex);
            var success_threshold = RANK_INCREMENT * primaryCriterias.Length * castGroups.Length; // 1 rank difference per criteria per cast group
            return new BranchAndBoundSolver<Applicant>(CostFunction, applicantVariables)
            {
                SuccessThreshold = success_threshold,
                StagnantTimeout = TIMEOUT_MS
            };
        }

        protected override Solution FindSatSolution(Solver<Applicant> sat, List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast,
            List<Clause<Applicant>> existing_assignments, List<Clause<Applicant>> same_cast_clauses, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            var clauses = new HashSet<Clause<Applicant>>();
            clauses.AddRange(existing_assignments);
            clauses.AddRange(same_cast_clauses);
            if (clauses.Count == 0)
            {
                if (sat.Variables.Count == 0)
                    return Solution.Unsolveable;
                var first_var = sat.Variables.First();
                clauses.Add(new()
                {
                    Literals = new[] {
                        Literal<Applicant>.Positive(first_var),
                        Literal<Applicant>.Negative(first_var)
                    }.ToHashSet()
                });
            }
            var solution = sat.Solve(new() { Clauses = clauses }).FirstOrDefault();
            return solution;
        }

        internal static void CalculateRankings(Criteria[] primaryCriterias, List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast, 
            out Applicant[] applicantVariables, out CastGroup[] castGroups, out int[][] applicantRanks, out int[] castGroupIndexFromVariableIndex)
        {
            applicantVariables = applicants_needing_alternative_cast.SelectMany(p => p.Item2).ToArray();
            castGroups = applicantVariables.Select(a => a.CastGroup!).Distinct().ToArray();
            applicantRanks = new int[primaryCriterias.Length][];
            castGroupIndexFromVariableIndex = new int[applicantVariables.Length];
            for (var c = 0; c < primaryCriterias.Length; c++)
            {
                applicantRanks[c] = new int[applicantVariables.Length];
                foreach (var (cast_group, applicants) in applicants_needing_alternative_cast)
                {
                    var cg = Array.IndexOf(castGroups, cast_group);
                    int rank = 0;
                    uint? previous = null;
                    foreach (var (applicant, mark) in applicants.Select(a => (a, a.MarkFor(primaryCriterias[c]))).OrderBy(p => p.Item2))
                    {
                        var av = Array.IndexOf(applicantVariables, applicant);
                        castGroupIndexFromVariableIndex[av] = cg;
                        if (mark != previous)
                        {
                            applicantRanks[c][av] = rank;
                            rank += RANK_INCREMENT;
                            previous = mark;
                        }
                        else
                            applicantRanks[c][av] = rank;
                    }
                }
            }
        }

        private (double lower, double upper) CostFunction(Solution partial_solution)
            => RankDifference(partial_solution, primaryCriterias, castGroups, applicantRanks, castGroupIndexFromVariableIndex);

        /// <summary>Calculates the bounds of the rank difference for a given partial solution.
        /// Returns double.MaxValue for both if the solution will not have even casts.</summary>
        internal static (double lower, double upper) RankDifference(Solution partial_solution, Criteria[] primaryCriterias, CastGroup[] castGroups, int[][] applicantRanks, int[] castGroupIndexFromVariableIndex)
        {
            var best_total = 0;
            var worst_total = 0;
            for (var c = 0; c < primaryCriterias.Length; c++)
            {
                var assigned_rank_difference = new int[castGroups.Length];
                var not_assigned_ranks = new List<int>[castGroups.Length];
                for (var cg = 0; cg < castGroups.Length; cg++)
                    not_assigned_ranks[cg] = new List<int>();
                var count_true = new int[castGroups.Length];
                var count_false = new int[castGroups.Length];
                var count_total = new int[castGroups.Length];
                for (var av = 0; av < partial_solution.Assignments.Length; av++)
                {
                    var cg = castGroupIndexFromVariableIndex[av];
                    var rank = applicantRanks[c][av];
                    if (partial_solution.Assignments[av] is not bool value)
                        not_assigned_ranks[cg].Add(rank);
                    else if (value)
                    {
                        assigned_rank_difference[cg] += rank;
                        count_true[cg]++;
                    }
                    else
                    {
                        assigned_rank_difference[cg] -= rank;
                        count_false[cg]++;
                    }
                    count_total[cg]++;
                }
                var best_criteria = 0;
                var worst_criteria = 0;
                for (var cg = 0; cg < castGroups.Length; cg++)
                {
                    int max = (count_total[cg] + 1) / 2;
                    if (count_true[cg] > max || count_false[cg] > max)
                        return (double.MaxValue, double.MaxValue); // invalid solution, casts are not even
                    var not_assigned_sorted = not_assigned_ranks[cg].OrderByDescending(r => r).ToArray();
                    var assignable_true = not_assigned_sorted.Take(max - count_true[cg]).Sum();
                    var assignable_false = not_assigned_sorted.Take(max - count_false[cg]).Sum();
                    var (best_cg, worst_cg) = FindBestAndWorstCases(assigned_rank_difference[cg], assignable_true, assignable_false);
                    best_criteria += best_cg;
                    worst_criteria += worst_cg;
                }
                best_total += best_criteria;
                worst_total += worst_criteria;
            }
            return (best_total, worst_total);
        }

        private static (int best, int worst) FindBestAndWorstCases(int current, int max_add, int max_subtract)
        {
            int best_case;
            if (current > 0 && max_subtract < current)
                best_case = current - max_subtract;
            else if (current < 0 && max_add < -current)
                best_case = -current - max_add;
            else
                best_case = 0;
            int worst_case = Math.Max(Math.Abs(current + max_add), Math.Abs(current - max_subtract));
            return (best_case, worst_case);
        }
    }
}

using Carmen.CastingEngine.SAT.Internal;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A concrete approach for balancing alternative casts by minimising difference of the sum of ranks
    /// for each criteria, between casts.
    /// NOTE: This only works for exactly 2 alternative casts
    /// </summary>
    public class RankDifferenceSatEngine : SatEngine
    {
        const int RANK_INCREMENT = 2;

        bool inProgress = false;
        int[][] applicantRanks = new int[0][]; // [criteria][applicant variable]
        int[] castGroupIndexFromVariableIndex = new int[0];
        CastGroup[] castGroups = new CastGroup[0];

        public RankDifferenceSatEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override Solver<Applicant> BuildSatSolver(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast)
        {
            var termination_threshold = RANK_INCREMENT * primaryCriterias.Length * castGroups.Length; // 1 rank difference per criteria per cast group
            //LATER try this without the termination threshold, or even consider a time based threshold
            return new BranchAndBoundSolver<Applicant>(CostFunction, termination_threshold, applicants_needing_alternative_cast.SelectMany(p => p.Item2).ToArray());
        }

        protected override Solution FindSatSolution(Solver<Applicant> sat, List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast,
            List<Clause<Applicant>> existing_assignments, List<Clause<Applicant>> same_cast_clauses, Dictionary<Applicant, SameCastSet> same_cast_lookup)
        {
            if (inProgress)
                throw new ApplicationException("FindSatSolution() already in progress");
            inProgress = true;
            // calculate and cache applicant ranks
            applicantRanks = new int[primaryCriterias.Length][];
            Applicant[] applicant_variables = sat.Variables.ToArray();
            castGroupIndexFromVariableIndex = new int[applicant_variables.Length];
            castGroups = applicant_variables.Select(a => a.CastGroup!).Distinct().ToArray();
            for (var c = 0; c < primaryCriterias.Length; c++)
            {
                applicantRanks[c] = new int[applicant_variables.Length];
                foreach (var (cast_group, applicants) in applicants_needing_alternative_cast)
                {
                    var cg = Array.IndexOf(castGroups, cast_group);
                    int rank = 0;
                    uint? previous = null;
                    foreach (var (applicant, mark) in applicants.Select(a => (a, a.MarkFor(primaryCriterias[c]))).OrderBy(p => p.Item2))
                    {
                        var av = Array.IndexOf(applicant_variables, applicant);
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
            // solve the sat
            var clauses = new HashSet<Clause<Applicant>>();
            clauses.AddRange(existing_assignments);
            clauses.AddRange(same_cast_clauses);
            var solution = sat.Solve(new() { Clauses = clauses }).FirstOrDefault();
            inProgress = false;
            return solution;
        }

        /// <summary>Calculates the rank difference for a given partial solution.
        /// Must also return double.MaxValue for solutions which don't have even casts.</summary>
        private (double lower, double upper) CostFunction(Solution partial_solution)
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

        private (int best, int worst) FindBestAndWorstCases(int current, int max_add, int max_subtract)
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

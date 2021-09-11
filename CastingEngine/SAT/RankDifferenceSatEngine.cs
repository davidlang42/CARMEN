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
        bool inProgress = false;
        Dictionary<(Applicant, Criteria), int> applicantRanks = new();
        Applicant[] applicantsByVariableIndex = new Applicant[0];
        CastGroup[] castGroupsByVariableIndex = new CastGroup[0];
        Solver<Applicant>? _sat;

        Solver<Applicant> sat => _sat ?? throw new ApplicationException($"Tried to access {nameof(sat)} before it has been initialized");

        public RankDifferenceSatEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override Solution FindSatSolution(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast,
            List<Clause<Applicant>> existing_assignments, List<Clause<Applicant>> same_cast_clauses, Dictionary<Applicant, SameCastSet> same_cast_lookup,
            out Solver<Applicant> sat)
        {
            if (inProgress)
                throw new ApplicationException("FindSatSolution() already in progress");
            inProgress = true;
            // calculate and cache applicant ranks
            applicantRanks.Clear();
            foreach (var criteria in primaryCriterias)
            {
                foreach (var (cg, applicants) in applicants_needing_alternative_cast)
                {
                    int rank = 0;
                    uint? previous = null;
                    foreach (var (applicant, mark) in applicants.Select(a => (a, a.MarkFor(criteria))).OrderBy(p => p.Item2))
                    {
                        if (mark != previous)
                        {
                            applicantRanks.Add((applicant, criteria), ++rank);
                            previous = mark;
                        }
                        else
                            applicantRanks.Add((applicant, criteria), rank);
                    }
                }
            }
            // build the sat
            this._sat = sat = new BranchAndBoundSolver<Applicant>(CostFunction, applicants_needing_alternative_cast.SelectMany(p => p.Item2));
            var clauses = new HashSet<Clause<Applicant>>();
            clauses.AddRange(existing_assignments);
            clauses.AddRange(same_cast_clauses);
            // cache variable mapping to applicant & cast group
            applicantsByVariableIndex = sat.Variables.ToArray();
            castGroupsByVariableIndex = applicantsByVariableIndex.Select(a => a.CastGroup!).ToArray();
            // solve the sat
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
            foreach (var criteria in primaryCriterias)
            {
                var assigned_rank_difference = new OmnificentDictionary<CastGroup, int>();
                var not_assigned_ranks = new List<(CastGroup, int)>();
                var count_true = new OmnificentDictionary<CastGroup, int>(); //TODO speed up by using arrays instead of dictionaries
                var count_false = new OmnificentDictionary<CastGroup, int>(); //TODO speed up by using arrays instead of dictionaries
                var count_total = new OmnificentDictionary<CastGroup, int>(); //TODO speed up by using arrays instead of dictionaries
                for (var i = 0; i < partial_solution.Assignments.Length; i++)
                {
                    var cg = castGroupsByVariableIndex[i];
                    var rank = applicantRanks[(applicantsByVariableIndex[i], criteria)]; //TODO speed up by using arrays instead of dictionaries
                    if (partial_solution.Assignments[i] is not bool value)
                        not_assigned_ranks.Add((cg, rank));
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
                foreach (var (cg, total) in count_total)
                {
                    int max = (total + 1) / 2;
                    if (count_true[cg] > max || count_false[cg] > max)
                        return (double.MaxValue, double.MaxValue); // invalid solution, casts are not even
                    var assignable_true = not_assigned_ranks.Where(p => p.Item1 == cg).Select(p => p.Item2).OrderByDescending(p => p).Take(max - count_true[cg]).Sum();
                    var assignable_false = not_assigned_ranks.Where(p => p.Item1 == cg).Select(p => p.Item2).OrderByDescending(p => p).Take(max - count_false[cg]).Sum();
                    var (best_cg, worst_cg) = FindBestAndWorstCases(assigned_rank_difference[cg], assignable_true, assignable_false); //TODO improve best/worst algorithm
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

using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A concrete approach for balancing alternative casts based on TopPairsSatEngine, but using a Branch & Bound algorithm
    /// to minimize the rank difference.
    /// </summary>
    public class BestPairsSatEngine : TopPairsSatEngine
    {
        int[][] applicantRanks = Array.Empty<int[]>(); // [criteria][applicant variable]
        Applicant[] applicantVariables = Array.Empty<Applicant>();
        int[] castGroupIndexFromVariableIndex = Array.Empty<int>();
        CastGroup[] castGroups = Array.Empty<CastGroup>();

        public BestPairsSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override Solver<Applicant> BuildSatSolver(List<(CastGroup, HashSet<Applicant>)> applicants_needing_alternative_cast)
        {
            RankDifferenceSatEngine.CalculateRankings(primaryCriterias, applicants_needing_alternative_cast, out applicantVariables, out castGroups, out applicantRanks, out castGroupIndexFromVariableIndex);
            return new BranchAndBoundSolver<Applicant>(CostFunction, applicantVariables)
            {
                StagnantTimeout = RankDifferenceSatEngine.TIMEOUT_MS
            };
        }

        private (double lower, double upper) CostFunction(Solution partial_solution)
            => RankDifferenceSatEngine.RankDifference(partial_solution, primaryCriterias, castGroups, applicantRanks, castGroupIndexFromVariableIndex);
    }
}

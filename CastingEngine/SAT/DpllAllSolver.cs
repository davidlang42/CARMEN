using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// An ALL-SAT solver based on the DPLL algorithm, but without pure literal propogation
    /// </summary>
    public class DpllAllSolver<T> : DpllSolver<T>
        where T : notnull
    {
        public DpllAllSolver(IEnumerable<T>? variables = null)
            : base(variables)
        {
            // if I want to keep pure literals and have all solutions, could possibly
            // back-check the inverse pure literal when returning solutions
            propogatePureLiterals = false;
        }

        protected override IEnumerable<Solution> BranchPartialSolve(HashSet<Clause<int>> remaining_clauses, Clause<int>[] branching_clauses, Solution partial_solution)
        {
            var solutions = new ConcurrentBag<Solution>();
            Parallel.ForEach(branching_clauses, branching_clause =>
            {
                var branch_clauses = new HashSet<Clause<int>>(remaining_clauses);
                branch_clauses.Add(branching_clause);
                var new_expression = new Expression<int>(branch_clauses);
                foreach (var solution in PartialSolve(new_expression, partial_solution))
                    solutions.Add(solution);
            });
            foreach (var solution in solutions)
                yield return solution;
        }
    }
}

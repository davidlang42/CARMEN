using System.Collections.Generic;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A really bad SAT solver
    /// </summary>
    public class BruteForceSolver<T> : Solver<T>
        where T : notnull
    {
        public BruteForceSolver(IEnumerable<T>? variables = null)
            : base(variables)
        { }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
            => PartialSolve(expression, new Solution { Assignments = new bool?[Variables.Count] }, 0);

        private IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution, int depth = 0)
        {
            partial_solution = partial_solution.Clone();
            if (depth == partial_solution.Assignments.Length)
            {
                if (Evaluate(expression, partial_solution))
                    yield return partial_solution;
            }
            else
            {
                partial_solution.Assignments[depth] = false;
                foreach (var solution in PartialSolve(expression, partial_solution, depth + 1))
                    yield return solution;
                partial_solution.Assignments[depth] = true;
                foreach (var solution in PartialSolve(expression, partial_solution, depth + 1))
                    yield return solution;
            }
        }
    }
}

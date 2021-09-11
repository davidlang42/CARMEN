using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// A weighted MAX-SAT solver based on the DPLL algorithm with branch & bound
    /// </summary>
    public class BranchAndBoundSolver<T> : DpllAllSolver<T>
        where T : notnull
    {
        /// <summary>Calculates the cost function as a range (between lower and upper bounds) for a given partial solution.
        /// For a solution which is fully assigned, the lower and upper bounds returned must be equal.</summary>
        public delegate (double lower, double upper) CostFunction(Solution partial_solution);

        private CostFunction costFunction; //LATER probably faster to split this into separate functions for lower and upper bounds, to reduce needless calculations of upper bound
        private double optimalCost;
        private readonly List<Solution> optimalSolutions = new();
        private bool inProgress = false;

        /// <summary>Finds the SAT solution which minimizes the given cost function</summary>
        public BranchAndBoundSolver(CostFunction cost_function, IEnumerable<T>? variables = null)
            : base(variables)
        {
            costFunction = cost_function;
        }

        public override IEnumerable<Solution> Solve(Expression<T> expression)
        {
            if (inProgress)
                throw new ApplicationException("Solve() already in progress");
            inProgress = true;
            optimalCost = double.MaxValue;
            optimalSolutions.Clear();
            foreach (var solution in base.Solve(expression))
            {
                var (lower, upper) = costFunction(solution);
                if (lower == upper)
                {
                    if (lower == optimalCost)
                        optimalSolutions.Add(solution);
                    else if (lower < optimalCost)
                    {
                        optimalCost = lower;
                        optimalSolutions.Clear();
                        optimalSolutions.Add(solution);
                    }
                }
                else
                {
                    foreach (var enumerated_solution in solution.Enumerate())
                    {
                        var (exact, confirm_exact) = costFunction(enumerated_solution);
                        if (exact != confirm_exact)
                            throw new ApplicationException($"Cost function did not return an exact value for a fully assigned solution: {enumerated_solution}");
                        if (exact == optimalCost)
                            optimalSolutions.Add(enumerated_solution);
                        else if (exact < optimalCost)
                        {
                            optimalCost = lower;
                            optimalSolutions.Clear();
                            optimalSolutions.Add(enumerated_solution);
                        }
                    }
                }
            }
            inProgress = false;
            foreach (var optimal_solution in optimalSolutions)
                yield return optimal_solution;
        }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
        {
            var (lower, upper) = costFunction(partial_solution);
            if (upper < lower)
                throw new ApplicationException($"Cost function returned an upper bound less than the lower bound for partial solution: {partial_solution}");
            if (lower > optimalCost)
                return Enumerable.Empty<Solution>();
            return base.PartialSolve(expression, partial_solution);
        }
    }
}

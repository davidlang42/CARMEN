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
        private double optimalUpper;
        private double optimalLower;
        private Solution optimalSolution;
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
            optimalUpper = double.MaxValue;
            optimalLower = double.MaxValue;
            optimalSolution = Solution.Unsolveable;
            var queue = new Queue<Solution>();
            var base_solutions = base.Solve(expression).GetEnumerator();
            if (base_solutions.MoveNext())
                queue.Enqueue(base_solutions.Current);
            while (queue.Any())
            {
                var solution = queue.Dequeue();
                var (lower, upper) = costFunction(solution);
                if (upper <= optimalLower)
                {
                    optimalLower = lower;
                    optimalUpper = upper;
                    optimalSolution = solution;
                }
                else if (lower < optimalUpper)
                {
                    if (upper - lower > optimalUpper - optimalLower)
                        Branch(solution, queue);
                    else
                    {
                        Branch(optimalSolution, queue);
                        queue.Enqueue(solution);
                        optimalLower = double.MaxValue;
                        optimalUpper = double.MaxValue;
                        optimalSolution = Solution.Unsolveable;
                    }
                }
                if (!queue.Any())
                {
                    if (base_solutions.MoveNext())
                        queue.Enqueue(base_solutions.Current);
                    else if (optimalUpper != optimalLower)
                    {
                        Branch(optimalSolution, queue);
                        queue.Enqueue(solution);
                        optimalLower = double.MaxValue;
                        optimalUpper = double.MaxValue;
                        optimalSolution = Solution.Unsolveable;
                    }
                }
            }
            inProgress = false;
            if (!optimalSolution.IsUnsolvable)
                yield return optimalSolution;
        }

        private void Branch(Solution solution, Queue<Solution> queue)
        {
            var first_unassigned = -1;
            for (var i = 0; i < solution.Assignments.Length; i++)
            {
                if (solution.Assignments[i] == null)
                {
                    first_unassigned = i;
                    break;
                }
            }
            if (first_unassigned == -1)
                throw new ApplicationException($"Cannot branch a fully assigned solution: {solution}");
            var new_solution = solution.Clone();
            new_solution.Assignments[first_unassigned] = false;
            queue.Enqueue(new_solution);
            new_solution = solution.Clone();
            new_solution.Assignments[first_unassigned] = true;
            queue.Enqueue(new_solution);
        }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
        {
            var (lower, upper) = costFunction(partial_solution);
            if (upper < lower)
                throw new ApplicationException($"Cost function returned an upper bound less than the lower bound for partial solution: {partial_solution}");
            if (lower > optimalUpper)
                return Enumerable.Empty<Solution>();
            return base.PartialSolve(expression, partial_solution);
        }
    }
}

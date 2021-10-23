using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.SAT
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

        private CostFunction costFunction;
        private double optimalUpper;
        private double optimalLower;
        private Solution optimalSolution;
        private DateTime optimalLastUpdated;
        private bool inProgress = false;

        /// <summary>If set, and the solver finds a complete solution with a cost function value below
        /// this threshold, it will immediately terminate without checking the remaining solutions.
        /// This is good for problems without difficult conditions to meet.</summary>
        public double? SuccessThreshold { get; set; }

        /// <summary>If set, and the solver runs for longer than the provided milliseconds without
        /// improving on the currently optimal complete solution, it will terminate without checking
        /// the remaining solutions. This is a good failsafe for usability.</summary>
        public int? StagnantTimeout { get; set; }

        /// <summary>Finds the SAT solution which minimizes the given cost function</summary>
        public BranchAndBoundSolver(CostFunction cost_function, IEnumerable<T>? variables = null)
            : base(variables)
        {
            costFunction = cost_function;
        }

        public override IEnumerable<Solution> SolveWithoutRemap(Expression<int> expression_int)
        {
            if (inProgress)
                throw new ApplicationException("Solve() already in progress");
            inProgress = true;
            optimalUpper = double.MaxValue;
            optimalLower = double.MaxValue;
            optimalSolution = Solution.Unsolveable;
            var stack = new Stack<Solution>();
            var base_solutions = base.SolveWithoutRemap(expression_int).GetEnumerator();
            if (base_solutions.MoveNext())
                stack.Push(base_solutions.Current);
            optimalLastUpdated = DateTime.Now;
            while (stack.Any() && !AbleToTerminate())
            {
                var solution = stack.Pop();
                var (lower, upper) = costFunction(solution);
                if (upper <= optimalLower)
                {
                    optimalLower = lower;
                    if (optimalUpper != upper)
                        optimalLastUpdated = DateTime.Now;
                    optimalUpper = upper;
                    optimalSolution = solution;
                }
                else if (lower < optimalUpper)
                {
                    if (upper - lower >= optimalUpper - optimalLower)
                        Branch(solution, stack);
                    else
                    {
                        Branch(optimalSolution, stack);
                        optimalLower = lower;
                        optimalUpper = upper;
                        optimalSolution = solution;
                        optimalLastUpdated = DateTime.Now;
                    }
                }
                if (!stack.Any())
                {
                    if (base_solutions.MoveNext())
                        stack.Push(base_solutions.Current);
                    else if (optimalUpper != optimalLower)
                    {
                        Branch(optimalSolution, stack);
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

        private bool AbleToTerminate()
            => !optimalSolution.IsUnsolvable && optimalLower == optimalUpper
            && (SuccessThreshold.HasValue && optimalLower <= SuccessThreshold.Value
            || StagnantTimeout.HasValue && (DateTime.Now - optimalLastUpdated).TotalMilliseconds > StagnantTimeout.Value);

        private void Branch(Solution solution, Stack<Solution> stack)
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
            stack.Push(new_solution);
            new_solution = solution.Clone();
            new_solution.Assignments[first_unassigned] = true;
            stack.Push(new_solution);
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

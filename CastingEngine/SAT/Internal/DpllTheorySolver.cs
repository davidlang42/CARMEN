﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// An SMT solver based on the DPLL(T) algorithm, extending DPLL with a domain specific validity test
    /// </summary>
    public class DpllTheorySolver<T> : DpllSolver<T> //LATER add unit tests
        where T : notnull
    {
        /// <summary>Tests the validity of a proposed solution, returning true if valid (for all possible
        /// assignments of remaining free variables), false if not valid (for any possible assignment of
        /// remaining free variables), or null if more assignments are required to determine validity</summary>
        public delegate bool? ValidityTest(Solution solution);

        private ValidityTest validityTest;

        public DpllTheorySolver(ValidityTest validity_test, IEnumerable<T>? variables = null)
            : base(variables)
        {
            validityTest = validity_test;
            propogatePureLiterals = false; //LATER if I want to keep pure literals and have all solutions, could possibly back-check the inverse pure literal when returning solutions
        }

        public override IEnumerable<Solution> Solve(Expression<T> expression)
        {
            foreach (var solution in base.Solve(expression))
            {
                var validity = validityTest(solution);
                if (validity == true)
                    yield return solution;
                else if (validity == null)
                    foreach (var enumerated_solution in solution.Enumerate())
                        if (validityTest(enumerated_solution) == true)
                            yield return enumerated_solution;
            }
        }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
        {
            if (validityTest(partial_solution) == false)
                return Enumerable.Empty<Solution>();
            return base.PartialSolve(expression, partial_solution);
        }
    }
}
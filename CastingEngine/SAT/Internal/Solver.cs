using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// An abstract SAT solver of the Boolean Satisfiability Problem.
    /// </summary>
    public abstract class Solver<T>
        where T : notnull
    {
        public SortedSet<T> Variables { get; init; }

        public Solver(IEnumerable<T>? variables = null)
        {
            Variables = variables == null ? new SortedSet<T>() : new SortedSet<T>(variables);
        }

        /// <summary>Introduces the solver to all variables in the given expression</summary>
        public void Introduce(Expression<T> expression)
        {
            foreach (var clause in expression.Clauses)
                foreach (var literal in clause.Literals)
                    Variables.Add(literal.Variable);
        }

        /// <summary>Checks than a boolean expression is valid. That is:
        /// - it only contains Variables known to the solver
        /// - it contains at least one Clause
        /// - it does not contain duplicate Clauses
        /// - each Clause contains at least one Literal
        /// - no Clause contains duplicate Literals</summary>
        public bool Check(Expression<T> expression)
        {
            if (expression.IsEmpty())
                return false;
            HashSet<Clause<T>> valid_clauses = new();
            foreach (var clause in expression.Clauses)
            {
                if (clause.IsEmpty())
                    return false;
                if (!valid_clauses.Add(clause))
                    return false;
                HashSet<Literal<T>> valid_literals = new();
                foreach (var literal in clause.Literals)
                {
                    if (!Variables.Contains(literal.Variable))
                        return false;
                    if (!valid_literals.Add(literal))
                        return false;
                }
            }
            return true;
        }

        /// <summary>Solves a boolean expression, or returns an empty sequence if it is found to be unsolveable</summary>
        public IEnumerable<Solution> Solve(Expression<T> expression)
        {
            if (!Check(expression))
                throw new ArgumentException($"{nameof(expression)} is not a valid boolean expression");
            int i = 0;
            var map = Variables.ToDictionary(v => v, v => i++);
            var expression_int = expression.Remap(map);
            foreach (var solution in PartialSolve(expression_int, new Solution { Assignments = new bool?[i] }))
                yield return solution;
        }

        protected abstract IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution);

        public bool Evaluate(Expression<T> expression, Solution full_solution)
        {
            if (!full_solution.IsFullyAssigned)
                throw new ArgumentException("Cannot evaulate a non-fully assigned solution");
            int i = 0;
            var map = Variables.ToDictionary(v => v, v => i++);
            var expression_int = expression.Remap(map);
            return Evaluate(expression_int, full_solution);
        }

        protected static bool Evaluate(Expression<int> expression, Solution full_solution)
            => expression.Clauses.All(c => c.Literals.Any(l => l.Polarity == full_solution.Assignments[l.Variable]));

        /// <summary>Maps a solution into pairs of Variable and Value</summary>
        public IEnumerable<Assignment<T>> MapAssignments(Solution solution)
        {
            if (solution.Assignments is not bool?[] assignments)
                throw new ArgumentException("Cannot map an unsolvable solution");
            foreach (var pair in Variables.Zip(assignments))
                yield return new Assignment<T> { Variable = pair.First, Value = pair.Second };
        }
    }
}

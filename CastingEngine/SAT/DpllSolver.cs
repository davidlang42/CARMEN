using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public class DpllSolver<T> : Solver<T>
        where T : notnull
    {
        public DpllSolver(IEnumerable<T>? variables = null)
            : base(variables)
        { }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
        {
            expression = expression.Clone();
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                Literal<int>? unit_clause_literal = null;
                foreach (var clause in expression.Clauses)
                {
                    if (clause.IsEmpty())
                        yield break; // unsolvable
                    if (clause.IsUnitClause(out unit_clause_literal))
                        break;
                }
                if (!unit_clause_literal.HasValue)
                    break; // no more unit clauses
                // Assign unit clause value
                partial_solution.Assignments[unit_clause_literal.Value.Variable] = unit_clause_literal.Value.Polarity;
                // Remove unit clause and any other clauses containing the unit clause literal
                expression.Clauses.RemoveWhere(c => c.Literals.Contains(unit_clause_literal.Value));
                // Remove inverse literal from any remaining clauses
                var inverse_literal = unit_clause_literal.Value.Inverse();
                foreach (var clause in expression.Clauses)
                    clause.Literals.RemoveWhere(l => l.Equals(inverse_literal));
            }
            // Check for solved
            if (expression.IsEmpty())
            {
                yield return partial_solution; // solved
                yield break;
            }
            // Propogate pure literals
            while (true)
            {
                // Find next pure literal
                var unique_literals = expression.Clauses.SelectMany(c => c.Literals).ToHashSet(); //LATER speed this up by grouping literals by variable and finding the first with only 1
                Literal<int>? pure_literal = null;
                foreach (var literal in unique_literals)
                {
                    if (!unique_literals.Contains(literal.Inverse()))
                    {
                        pure_literal = literal;
                        break;
                    }
                }
                if (!pure_literal.HasValue)
                    break; // no more pure literals
                // Assign pure literal value (might not be required, but is always safe)
                partial_solution.Assignments[pure_literal.Value.Variable] = pure_literal.Value.Polarity;
                // Remove all clauses containing the pure literal
                expression.Clauses.RemoveWhere(c => c.Literals.Contains(pure_literal.Value));
            }
            // Check for solved (again)
            if (expression.IsEmpty())
            {
                yield return partial_solution; // solved
                yield break;
            }
            // Pick an unassigned literal and branch
            var unassigned_literal = expression.Clauses.First().Literals.First(); //LATER speed this up by branching on the most common literal
            var branching_clause = new Clause<int> { Literals = unassigned_literal.Yield().ToHashSet() };
            expression.Clauses.Add(branching_clause);
            foreach (var solution in PartialSolve(expression, partial_solution))
                yield return solution; // propogate any found solutions
            expression.Clauses.Remove(branching_clause);
            branching_clause = new Clause<int> { Literals = unassigned_literal.Inverse().Yield().ToHashSet() };
            expression.Clauses.Add(branching_clause);
            foreach (var solution in PartialSolve(expression, partial_solution))
                yield return solution; // propogate any found solutions
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public class DpllSolver : Solver
    {
        public DpllSolver(IEnumerable<Variable> variables)
            : base(variables.ToHashSet())
        { }

        public override Solution? Solve(Expression expression)
        {
            if (PartialSolve(expression) is Assignment[] assignments)
            {
                var free_variables = Variables.ToHashSet();
                foreach (var a in assignments)
                    free_variables.Remove(a.Variable);
                return new Solution { Assignments = assignments.ToArray(), FreeVariables = free_variables.ToArray() };
            }
            return null;
        }

        private static Assignment[]? PartialSolve(Expression expression)
        {
            var assignments = new List<Assignment>();
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                Literal? unit_clause_literal = null;
                foreach (var clause in expression.Clauses)
                {
                    if (clause.IsEmpty())
                        return null; // unsolvable
                    if (clause.IsUnitClause(out unit_clause_literal))
                        break;
                }
                if (!unit_clause_literal.HasValue)
                    break; // no more unit clauses
                // Assign unit clause value
                assignments.Add(unit_clause_literal.Value.AssignmentForTrue());
                // Remove unit clause and any other clauses containing the unit clause literal
                expression.Clauses = expression.Clauses.Where(c => !c.Literals.Contains(unit_clause_literal.Value)).ToArray();
                // Remove inverse literal from any remaining clauses
                var inverse_literal = unit_clause_literal.Value.InverseLiteral();
                for (var i = 0; i < expression.Clauses.Length; i++)
                    expression.Clauses[i].Literals = expression.Clauses[i].Literals.Where(l => !l.Equals(inverse_literal)).ToArray();
            }
            // Check for solved
            if (expression.IsEmpty())
                return assignments.ToArray(); // solved
            // Propogate pure literals
            while (true)
            {
                // Find next pure literal
                var unique_literals = expression.Clauses.SelectMany(c => c.Literals).ToHashSet();
                Literal? pure_literal = null;
                foreach (var literal in unique_literals)
                {
                    if (!unique_literals.Contains(literal.InverseLiteral()))
                    {
                        pure_literal = literal;
                        break;
                    }
                }
                if (!pure_literal.HasValue)
                    break; // no more pure literals
                // Assign pure literal value (might not be required, but is always safe)
                assignments.Add(pure_literal.Value.AssignmentForTrue());
                // Remove all clauses containing the pure literal
                expression.Clauses = expression.Clauses.Where(c => !c.Literals.Contains(pure_literal.Value)).ToArray();
            }
            // Check for solved (again)
            if (expression.IsEmpty())
                return assignments.ToArray(); // solved
            // Pick an unassigned literal and branch
            var unassigned_literal = expression.Clauses[0].Literals[0];
            Assignment[]? partial_assignments = PartialSolve(new Expression { Clauses = expression.Clauses.Concat(new Clause { Literals = new[] { unassigned_literal } }.Yield()).ToArray() })
                ?? PartialSolve(new Expression { Clauses = expression.Clauses.Concat(new Clause { Literals = new[] { unassigned_literal.InverseLiteral() } }.Yield()).ToArray() });
            return partial_assignments?.Concat(assignments).ToArray(); // propogate solved/unsolved
        }   
    }
}

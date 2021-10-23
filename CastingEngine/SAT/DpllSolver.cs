using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A SAT solver implementing the DPLL algorithm
    /// </summary>
    public class DpllSolver<T> : Solver<T>
        where T : notnull
    {
        /// <summary>Propogating pure literals may cause skipping some solutions,
        /// set to false to ensure every solution is enumerated.</summary>
        protected bool propogatePureLiterals { get; set; } = true;

        public DpllSolver(IEnumerable<T>? variables = null)
            : base(variables)
        { }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution)
        {
            partial_solution = partial_solution.Clone();
            var clauses = expression.Clauses.ToHashSet();
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                var unit = FindUnitClause(clauses); //TODO process many units at once
                if (unit.Solved)
                    yield return partial_solution;
                if (unit.Solved || unit.Failed)
                    yield break;
                if (!unit.Found)
                    break;
                // Assign unit clause value
                partial_solution.Assignments[unit.Literal.Variable] = unit.Literal.Polarity;
                // Remove unit clause and any other clauses containing the unit clause literal
                clauses.RemoveWhere(c => c.Literals.Contains(unit.Literal)); //TODO try hacking the next check into this
                // Remove inverse literal from any remaining clauses
                var inverse_literal = unit.Literal.Inverse();
                var containing_inverse_literal = clauses.Where(c => c.Literals.Contains(inverse_literal)).ToList();
                foreach (var old_clause in containing_inverse_literal)
                {
                    clauses.Remove(old_clause);
                    clauses.Add(old_clause with { Literals = old_clause.Literals.Where(l => l != inverse_literal).ToHashSet() });
                }
            }
            // Propogate pure literals
            if (propogatePureLiterals)
            {
                while (true)
                {
                    // Find next pure literal
                    var pure = FindPureLiteral(clauses); //TODO process many pures at once
                    if (pure.Solved)
                        yield return partial_solution;
                    if (pure.Solved || pure.Failed)
                        yield break;
                    if (!pure.Found)
                        break;
                    // Assign pure literal value (might not be required, but is always safe)
                    partial_solution.Assignments[pure.Literal.Variable] = pure.Literal.Polarity;
                    // Remove all clauses containing the pure literal
                    clauses.RemoveWhere(c => c.Literals.Contains(pure.Literal));
                }
            }
            // Pick an unassigned literal and branch
            var unassigned_literal = clauses.First().Literals.First();
            var branching_clause = Clause<int>.Unit(unassigned_literal);
            clauses.Add(branching_clause);
            var new_expression = new Expression<int>(clauses); //TODO reuse expression
            foreach (var solution in PartialSolve(new_expression, partial_solution))
                yield return solution; // propogate any found solutions
            clauses.Remove(branching_clause);
            branching_clause = Clause<int>.Unit(unassigned_literal.Inverse());//TODO reuse branching clause?
            clauses.Add(branching_clause);
            foreach (var solution in PartialSolve(new_expression, partial_solution))
                yield return solution; // propogate any found solutions
        }

        private struct SearchResult
        {
            public bool Solved { get; set; }
            public bool Failed { get; set; }
            public bool Found { get; set; }
            public Literal<int> Literal { get; set; }

            public static SearchResult Fail() => new() { Failed = true };
            public static SearchResult Solve() => new() { Solved = true };
            public static SearchResult Find(Literal<int> literal) => new()
            {
                Found = true,
                Literal = literal
            };
        }

        private static SearchResult FindUnitClause(HashSet<Clause<int>> clauses)
        {
            if (clauses.Count == 0)
                return SearchResult.Solve();
            foreach (var clause in clauses)
            {
                if (clause.IsEmpty())
                    return SearchResult.Fail();
                if (clause.IsUnitClause(out var literal))
                    return SearchResult.Find(literal);
            }
            return default;
        }

        private static SearchResult FindPureLiteral(HashSet<Clause<int>> clauses)
        {
            if (clauses.Count == 0)
                return SearchResult.Solve();
            var unique_literals = clauses.SelectMany(c => c.Literals).ToHashSet();//TODO is it faster to group by variable?
            foreach (var literal in unique_literals)
            {
                if (!unique_literals.Contains(literal.Inverse()))
                    return SearchResult.Find(literal);
            }
            return default;
        }
    }
}

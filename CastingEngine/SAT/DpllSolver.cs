using Carmen.ShowModel;
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

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution) //TODO NEW APPROACH: real speed up approach would be using multi-dim arrays to store flags of whether or not that var is referenced in the clause/literal
        {
            partial_solution = partial_solution.Clone();
            var old_clauses = expression.Clauses.ToHashSet(); // clauses which haven't been modified yet (don't modify, only remove)
            var new_clauses = new HashSet<Clause<int>>(); // clauses which have been created in this step (safe to modify)
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                var unit = FindUnitClause(old_clauses.Concat(new_clauses));
                if (unit.Solved)
                    yield return partial_solution;
                if (unit.Solved || unit.Failed)
                    yield break;
                if (!unit.Found)
                    break;
                // Assign unit clause value
                partial_solution.Assignments[unit.Literal.Variable] = unit.Literal.Polarity;
                // Remove unit clause and any other clauses containing the unit clause literal
                old_clauses.RemoveWhere(c => c.Literals.Contains(unit.Literal));
                new_clauses.RemoveWhere(c => c.Literals.Contains(unit.Literal));
                // Remove inverse literal from any remaining clauses
                var inverse_literal = unit.Literal.Inverse();
                foreach (var new_clause in new_clauses)
                    new_clause.Literals.Remove(inverse_literal);
                old_clauses.RemoveWhere(old_clause =>
                {
                    if (old_clause.Literals.Contains(inverse_literal))
                    {
                        var new_literals = new HashSet<Literal<int>>(old_clause.Literals);
                        new_literals.Remove(inverse_literal);
                        new_clauses.Add(new(new_literals));
                        return true;
                    }
                    return false;
                });
            }
            new_clauses.AddRange(old_clauses); // from here on we don't modify clauses, so combine the sets
            // Propogate pure literals
            if (propogatePureLiterals)
            {
                while (true)
                {
                    // Find next pure literal
                    var pure = FindPureLiteral(new_clauses);
                    if (pure.Solved)
                        yield return partial_solution;
                    if (pure.Solved || pure.Failed)
                        yield break;
                    if (!pure.Found)
                        break;
                    // Assign pure literal value (might not be required, but is always safe)
                    foreach (var pure_literal in pure.Literals)
                        partial_solution.Assignments[pure_literal.Variable] = pure_literal.Polarity;
                    // Remove all clauses containing the pure literal
                    new_clauses.RemoveWhere(c => pure.Literals.Any(pl => c.Literals.Contains(pl)));
                }
            }
            // Pick an unassigned literal and branch
            var unassigned_literal = new_clauses.First().Literals.First();
            var branching_clauses = new[]
            {
                Clause<int>.Unit(unassigned_literal),
                Clause<int>.Unit(unassigned_literal.Inverse())
            };
            foreach (var branching_clause in branching_clauses)
            {
                var branch_clauses = new HashSet<Clause<int>>(new_clauses);
                branch_clauses.Add(branching_clause);
                var new_expression = new Expression<int>(branch_clauses);
                foreach (var solution in PartialSolve(new_expression, partial_solution))
                    yield return solution; // propogate any found solutions
            }
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

        private struct SearchResults
        {
            public bool Solved { get; set; }
            public bool Failed { get; set; }
            public bool Found { get; set; }
            public List<Literal<int>> Literals { get; set; }

            public static SearchResults Fail() => new() { Failed = true };
            public static SearchResults Solve() => new() { Solved = true };
            public static SearchResults Find(List<Literal<int>> literals) => new()
            {
                Found = true,
                Literals = literals
            };
        }

        private static SearchResult FindUnitClause(IEnumerable<Clause<int>> clauses)
        {
            bool any = false;
            foreach (var clause in clauses)
            {
                if (clause.IsEmpty())
                    return SearchResult.Fail();
                if (clause.IsUnitClause(out var literal))
                    return SearchResult.Find(literal);
                any = true;
            }
            if (!any)
                return SearchResult.Solve();
            return default;
        }

        private static SearchResults FindPureLiteral(HashSet<Clause<int>> clauses)
        {
            if (clauses.Count == 0)
                return SearchResults.Solve();
            var pure_by_variable = new Dictionary<int, Literal<int>?>();
            foreach (var literal in clauses.SelectMany(c => c.Literals))
            {
                if (pure_by_variable.TryGetValue(literal.Variable, out var existing_literal))
                {
                    if (existing_literal != null && existing_literal.Polarity != literal.Polarity)
                        // variable already referenced as inverse literal, therefore not pure
                        pure_by_variable[literal.Variable] = null;
                }
                else
                    pure_by_variable.Add(literal.Variable, literal);
            }
            var pure_literals = pure_by_variable.Values.NotNull().ToList();
            if (pure_literals.Any())
                return SearchResults.Find(pure_literals);
            return default;
        }
    }
}

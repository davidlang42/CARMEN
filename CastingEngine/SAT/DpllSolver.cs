using Carmen.ShowModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var clauses = expression.Clauses.ToHashSet(); // don't modify clauses, only remove
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                var unit = FindUnitClause(clauses);
                if (unit.Solved)
                    yield return partial_solution;
                if (unit.Solved || unit.Failed)
                    yield break;
                if (!unit.Found)
                    break;
                // Assign unit clause value
                partial_solution.Assignments[unit.Literal.Variable] = unit.Literal.Polarity;
                // Remove unit clause and any other clauses containing the unit clause literal
                clauses.RemoveWhere(c => c.Literals.Contains(unit.Literal));
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
                    var pure = FindPureLiteral(clauses);
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
                    clauses.RemoveWhere(c => pure.Literals.Any(pl => c.Literals.Contains(pl)));
                }
            }
            // Pick an unassigned literal and branch
            var unassigned_literal = clauses.First().Literals.First();
            var branching_clauses = new[]
            {
                Clause<int>.Unit(unassigned_literal),
                Clause<int>.Unit(unassigned_literal.Inverse())
            };
            foreach (var solution in BranchPartialSolve(clauses, branching_clauses, partial_solution))
                yield return solution;
        }

        protected virtual IEnumerable<Solution> BranchPartialSolve(HashSet<Clause<int>> remaining_clauses, Clause<int>[] branching_clauses, Solution partial_solution)
        {
            foreach (var branching_clause in branching_clauses)
            {
                var branch_clauses = new HashSet<Clause<int>>(remaining_clauses);
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

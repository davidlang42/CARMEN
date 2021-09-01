using Carmen.CastingEngine.SAT.Internal;
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
        /// <summary>Propogating pure literals may cause skipping some solutions.
        /// If every solution is needed, set SkipPureLiterals to true.</summary>
        public bool SkipPureLiterals { get; set; } = false;

        public DpllSolver(IEnumerable<T>? variables = null)
            : base(variables)
        { }

        protected override IEnumerable<Solution> PartialSolve(Expression expression, Solution partial_solution)
        {
            partial_solution = partial_solution.Clone();
            // Propogate unit clauses
            while (true)
            {
                // Find next unit clause literal
                var unit = FindUnitClause(expression);
                if (unit.Solved)
                    yield return partial_solution; // solved
                if (unit.Failed || unit.Solved)
                    yield break; // unsolvable / solved
                if (!unit.Found)
                    break; // no more unit clauses
                // Assign unit clause value
                partial_solution.Assignments[unit.Variable] = unit.Literal == 1;
                // Remove unit clause and any other clauses containing the unit clause literal
                expression.Clauses = expression.Clauses.Where(c => !c.Variables[unit.Variable].Literals[unit.Literal]).ToArray();
                // Remove inverse literal from any remaining clauses
                var inverse_literal = 1 - unit.Literal;
                for (var j = 0; j < expression.Clauses.Length; j++)
                    if (expression.Clauses[j].Variables[unit.Variable].Literals[inverse_literal])
                        expression.Clauses[j] = expression.Clauses[j].Without(unit.Variable);
            }
            // Propogate pure literals
            if (!SkipPureLiterals) //LATER if I want to keep pure literals and have all solutions, could possibly back-check the inverse pure literal when returning solutions
            {
                while (true)
                {
                    // Find next pure literal
                    var pure = FindPureLiteral(expression);
                    if (pure.Solved)
                        yield return partial_solution; // solved
                    if (pure.Failed || pure.Solved)
                        yield break; // unsolvable / solved
                    if (!pure.Found)
                        break; // no more pure clauses
                    // Assign pure literal value (might not be required, but is always safe)
                    partial_solution.Assignments[pure.Variable] = pure.Literal == 1;
                    // Remove all clauses containing the pure literal
                    expression.Clauses = expression.Clauses.Where(c => !c.Variables[pure.Variable].Literals[pure.Literal]).ToArray();
                }
            }
            // Pick an unassigned literal and branch
            var (clause, inverse_clause) = FindBranchingClauses(expression);
            foreach (var solution in PartialSolve(expression.With(clause), partial_solution))
                yield return solution; // propogate any found solutions
            foreach (var solution in PartialSolve(expression.With(inverse_clause), partial_solution))
                yield return solution; // propogate any found solutions
        }

        private struct SearchResult
        {
            public bool Solved { get; set; }
            public bool Failed { get; set; }
            public bool Found { get; set; }
            public int Variable { get; set; }
            public int Literal { get; set; }

            public static SearchResult Fail() => new() { Failed = true };
            public static SearchResult Solve() => new() { Solved = true };
            public static SearchResult Find(int variable, int literal) => new()
            {
                Found = true,
                Variable = variable,
                Literal = literal
            };
        }

        private static SearchResult FindUnitClause(Expression exp)
        {
            if (exp.Clauses.Length == 0)
                return SearchResult.Solve();
            int n_variables = exp.Clauses[0].Variables.Length;
            const int n_literals = 2;
            bool empty_clause, unit_clause;
            int variable = 0;
            int literal = 0;
            for (var c = 0; c < exp.Clauses.Length; c++)
            {
                empty_clause = true;
                unit_clause = false;
                for (var v=0; v < n_variables; v++)
                {
                    for (var l = 0; l < n_literals; l++)
                    {
                        if (exp.Clauses[c].Variables[v].Literals[l])
                        {
                            if (empty_clause)
                            {
                                empty_clause = false;
                                unit_clause = true;
                                variable = v;
                                literal = l;
                            }
                            else if (unit_clause)
                            {
                                unit_clause = false;
                                break;
                            }
                        }
                    }
                    if (!empty_clause && !unit_clause)
                        break;
                }
                if (empty_clause)
                    return SearchResult.Fail();
                if (unit_clause)
                    return SearchResult.Find(variable, literal);
            }
            return default;
        }

        private static SearchResult FindPureLiteral(Expression exp)
        {
            if (exp.Clauses.Length == 0)
                return SearchResult.Solve();
            int n_variables = exp.Clauses[0].Variables.Length;
            const int n_literals = 2;
            bool[] literal_used;
            for (var v = 0; v < n_variables; v++)
            {
                literal_used = new bool[n_literals];
                for (var l = 0; l < n_literals; l++)
                {
                    for (var c = 0; c < exp.Clauses.Length; c++)
                    {
                        if (exp.Clauses[c].Variables[v].Literals[l])
                        {
                            literal_used[l] = true;
                            break;
                        }
                    }
                }
                if (literal_used[0] != literal_used[1])
                    return SearchResult.Find(v, literal_used[0] ? 0 : 1);
            }
            return default;
        }

        private static (Clause, Clause) FindBranchingClauses(Expression exp)
        {
            int n_variables = exp.Clauses[0].Variables.Length;
            const int n_literals = 2;
            //LATER speed this up by branching on the most common literal
            for (var c = 0; c < exp.Clauses.Length; c++)
                for (var v = 0; v < n_variables; v++)
                    for (var l = 0; l < n_literals; l++)
                        if (exp.Clauses[c].Variables[v].Literals[l])
                            return (Clause.Unit(n_variables, n_literals, v, l), Clause.Unit(n_variables, n_literals, v, 1 - l));
            throw new ApplicationException("No unassigned literals found");
        }
    }
}

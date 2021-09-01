using Carmen.CastingEngine.SAT;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.SAT
{
    public abstract class SolverTests
    {
        protected void TestSolve(Solver<int> sat, Expression<int> expression, bool expected_solvable)
        {
            var solution = sat.Solve(expression).FirstOrDefault();
            if (expected_solvable)
            {
                solution.IsUnsolvable.Should().BeFalse();
                sat.Evaluate(expression, solution.FullyAssigned(false)).Should().BeTrue();
                sat.Evaluate(expression, solution.FullyAssigned(true)).Should().BeTrue();
            }
            else
            {
                solution.IsUnsolvable.Should().BeTrue();
            }
        }

        protected Expression<T> GenerateExpression<T>(int random_seed, T[] variables, int j_clauses, int k_literals_per_clause)
            where T : notnull
        {
            var random = new Random(random_seed);
            var all_literals = variables.SelectMany(v => new[] { Literal<T>.Positive(v), Literal<T>.Negative(v) }).ToArray();
            var clauses = new List<Clause<T>>();
            for (var j = 0; j < j_clauses; j++)
            {
                var literals = new List<Literal<T>>();
                for (var k = 0; k < k_literals_per_clause; k++)
                {
                    literals.Add(all_literals[random.Next(all_literals.Length)]);
                }
                clauses.Add(new Clause<T> { Literals = literals.ToHashSet() });
            }
            return new Expression<T> { Clauses = clauses.ToHashSet() };
        }
    }
}

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
        protected Variable[] GenerateVariables(int count)
        {
            var variables = new Variable[count];
            for (var i = 0; i < count; i++)
                variables[i] = new NumberedVariable { Number = (uint)i + 1 };
            return variables;
        }

        protected void TestSolve(Solver sat, Expression expression, bool expected_solvable)
        {
            var solution = sat.Solve(expression);
            if (expected_solvable)
            {
                solution.Should().NotBeNull();
                expression.Evaluate(solution!.Value.FullyAssigned(false)).Should().BeTrue();
                expression.Evaluate(solution.Value.FullyAssigned(true)).Should().BeTrue();
            }
            else
            {
                solution.Should().BeNull();
            }
        }

        protected Expression GenerateExpression(int random_seed, Variable[] variables, int j_clauses, int k_literals_per_clause)
        {
            var random = new Random(random_seed);
            var all_literals = variables.SelectMany(v => new[] { v.PositiveLiteral, v.NegativeLiteral }).ToArray();
            var clauses = new List<Clause>();
            for (var j = 0; j < j_clauses; j++)
            {
                var literals = new List<Literal>();
                for (var k = 0; k < k_literals_per_clause; k++)
                {
                    literals.Add(all_literals[random.Next(all_literals.Length)]);
                }
                clauses.Add(new Clause { Literals = literals.ToArray() });
            }
            return new Expression { Clauses = clauses.ToArray() };
        }
    }
}

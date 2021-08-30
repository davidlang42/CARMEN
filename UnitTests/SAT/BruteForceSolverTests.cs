using Carmen.CastingEngine.SAT;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.SAT
{
    public class BruteForceSolverTests : SolverTests
    {
        [Test]
        public void Simple_Solvable()
        {
            var v = GenerateVariables(2);
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            v[0].PositiveLiteral,
                            v[1].PositiveLiteral
                        }
                    },
                    new Clause
                    {
                        Literals = new[]
                        {
                            v[0].NegativeLiteral,
                            v[1].NegativeLiteral
                        }
                    }
                }
            };
            TestSolve(new BruteForceSolver(v), expression, true);
        }

        [Test]
        public void Simple_Unsolvable()
        {
            var v = GenerateVariables(1);
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            v[0].PositiveLiteral
                        }
                    },
                    new Clause
                    {
                        Literals = new[]
                        {
                            v[0].NegativeLiteral
                        }
                    }
                }
            };
            TestSolve(new BruteForceSolver(v), expression, false);
        }

        [Test]
        [TestCase(100, 10, 50, 3)]
        public void Simple_Random(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = GenerateVariables(n_variables);
            var sat = new BruteForceSolver(vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (sat.Solve(expression) is Solution solution)
                {
                    expression.Evaluate(solution.FullyAssigned(false)).Should().BeTrue();
                    expression.Evaluate(solution.FullyAssigned(true)).Should().BeTrue();
                    solved++;
                }
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }
    }
}

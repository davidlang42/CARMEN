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
    public class DpllSolverTests : SolverTests
    {
        [Test]
        public void Simple_Solvable()
        {
            var v = new[] { 1, 2 };
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(v[0]),
                            Literal<int>.Positive(v[1])
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Negative(v[0]),
                            Literal<int>.Negative(v[1])
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            TestSolve(new DpllSolver<int>(v), expression, true);
        }

        [Test]
        public void Simple_Unsolvable()
        {
            var v = new[] { 1 };
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(v[0])
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Negative(v[0])
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            TestSolve(new DpllSolver<int>(v), expression, false);
        }

        [Test]
        [TestCase(100, 10, 50, 3, TestName = "10_Vars_Easy")] // 60ms
        [TestCase(100, 15, 70, 3, TestName = "15_Vars_Medium")] // 80ms
        [TestCase(100, 20, 90, 3, TestName = "20_Vars_Hard")] // 150ms
        [TestCase(100, 25, 110, 3, TestName = "25_Vars_VeryHard")] // 270ms
        //[TestCase(100, 50, 210, 3, TestName = "50_Vars_Extreme")] // 6.3s
        public void Random(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new DpllSolver<int>(vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression<int>(seed, vars, j_clauses, k_literals);
                Solution solution = sat.Solve(expression).FirstOrDefault();
                if (!solution.IsUnsolvable)
                {
                    sat.Evaluate(expression, solution.FullyAssigned(false)).Should().BeTrue();
                    sat.Evaluate(expression, solution.FullyAssigned(true)).Should().BeTrue();
                    solved++;
                }
                //TODO check if false
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }
    }
}

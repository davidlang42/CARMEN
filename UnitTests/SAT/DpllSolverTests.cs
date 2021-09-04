using Carmen.CastingEngine.SAT;
using Carmen.CastingEngine.SAT.Internal;
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
            TestSolve(new DpllSolver<int>(v), expression).Should().BeTrue();
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
            TestSolve(new DpllSolver<int>(v), expression).Should().BeFalse();
        }

        [Test]
        [TestCase(100, 10, 50, 3, TestName = "10_Vars_Easy")] // 40ms
        [TestCase(100, 15, 70, 3, TestName = "15_Vars_Medium")] // 80ms
        [TestCase(100, 20, 90, 3, TestName = "20_Vars_Hard")] // 160ms
        [TestCase(100, 25, 110, 3, TestName = "25_Vars_VeryHard")] // 260ms
        //[TestCase(100, 50, 210, 3, TestName = "50_Vars_Extreme")] // 4.5s
        public void Random(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new DpllSolver<int>(vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (TestSolve(sat, expression))
                    solved++;
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }

        [Test]
        [TestCase(100, 10, 50, 3, TestName = "All_10_Vars_Easy")] // 40ms
        [TestCase(100, 15, 70, 3, TestName = "All_15_Vars_Medium")] // 100ms
        [TestCase(100, 20, 90, 3, TestName = "All_20_Vars_Hard")] // 200ms
        [TestCase(100, 25, 110, 3, TestName = "All_25_Vars_VeryHard")] // 450ms
        //[TestCase(100, 50, 210, 3, TestName = "All_50_Vars_Extreme")] // 8.9s
        public void RandomAll(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new DpllSolver<int>(vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (TestSolveAll(sat, expression))
                    solved++;
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }

        [Test]
        [TestCase(100, 10, 50, 3, TestName = "Count_10_Vars_Easy")] // 200ms
        //[TestCase(100, 15, 70, 3, TestName = "Count_15_Vars_Medium")] // 3.1s
        public void CompareCounts(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var dpll = new DpllSolver<int>(vars) { SkipPureLiterals = true };
            var brute = new BruteForceSolver<int>(vars);
            var maximum_solutions = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                var dpll_sol = dpll.Solve(expression).ToArray();
                var brute_sol = brute.Solve(expression).ToArray();
                var dpll_all = dpll_sol.SelectMany(s => s.Enumerate()).ToArray();
                var brute_all = brute_sol.SelectMany(s => s.Enumerate()).ToArray();
                var dpll_count = dpll_all.Length;
                var brute_count = brute_all.Length;
                dpll_count.Should().Be(brute_count);
                if (dpll_count > maximum_solutions)
                    maximum_solutions = dpll_count;
            }
            maximum_solutions.Should().BeGreaterThan(1); // otherwise we didn't really test anything
        }
    }
}

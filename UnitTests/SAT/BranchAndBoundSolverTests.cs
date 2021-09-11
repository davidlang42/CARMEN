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
    public class BranchAndBoundSolverTests : SolverTests
    {
        /// <summary>Calculates cost as the number of true assignments</summary>
        private (double, double) CostFunction(Solution solution)
        {
            int count_true = 0;
            int count_false = 0;
            int count_null = 0;
            for (var i = 0; i < solution.Assignments.Length; i++)
                if (solution.Assignments[i] is not bool value)
                    count_null++;
                else if (value)
                    count_true++;
                else
                    count_false++;
            return (count_true, count_true + count_null);
        }

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
            TestSolveMinimum(new BranchAndBoundSolver<int>(CostFunction, v), expression, CostFunction).Should().BeTrue();
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
            TestSolveMinimum(new BranchAndBoundSolver<int>(CostFunction, v), expression, CostFunction).Should().BeFalse();
        }

        [Test]
        [TestCase(100, 10, 50, 3, TestName = "10_Vars_Easy")] // 100ms
        [TestCase(100, 15, 70, 3, TestName = "15_Vars_Medium")] // 140ms
        [TestCase(100, 20, 90, 3, TestName = "20_Vars_Hard")] // 270ms
        [TestCase(100, 25, 110, 3, TestName = "25_Vars_VeryHard")] // 620ms
        //[TestCase(100, 50, 210, 3, TestName = "50_Vars_Extreme")] // 13.4s
        public void Random(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new BranchAndBoundSolver<int>(CostFunction, vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (TestSolveMinimum(sat, expression, CostFunction))
                    solved++;
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }

        [Test]
        [TestCase(100, 10, 10, 3, TestName = "Speed10")] // 30ms
        [TestCase(100, 15, 15, 3, TestName = "Speed15")] // 110ms
        [TestCase(100, 20, 20, 3, TestName = "Speed20")] // 300ms
        [TestCase(100, 25, 25, 3, TestName = "Speed25")] // 650ms
        [TestCase(100, 30, 30, 3, TestName = "Speed30")] // 1.7s
        [TestCase(100, 35, 35, 3, TestName = "Speed35")] // 5.3s
        //[TestCase(100, 40, 40, 3, TestName = "Speed40")] // 21s
        //[TestCase(100, 45, 45, 3, TestName = "Speed45")] // 1.4m
        public void SpeedTest(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new BranchAndBoundSolver<int>(CostFunction, vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (TestSolve(sat, expression))
                    solved++;
            }
            var percent_solved = solved * 100 / test_cases;
            Console.WriteLine($"Percent solved: {percent_solved:0}%");
        }
    }
}

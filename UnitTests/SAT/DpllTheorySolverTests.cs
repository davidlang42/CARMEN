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
    public class DpllTheorySolverTests : SolverTests
    {
        [Test]
        public void Simple_Solvable_Valid()
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
            TestSolve(new DpllTheorySolver<int>(TestWhichMakesItSolveable, v), expression).Should().BeTrue();
        }

        private bool? TestWhichMakesItSolveable(Solution soln)
        {
            if (soln.Assignments[0] == null || soln.Assignments[1] == null)
                return null;
            return soln.Assignments[0] != soln.Assignments[1];
        }

        [Test]
        public void Simple_Solvable_Invalid()
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
            TestSolve(new DpllTheorySolver<int>(TestWhichMakesItUnsolveable, v), expression).Should().BeFalse();
        }

        private bool? TestWhichMakesItUnsolveable(Solution soln)
        {
            if (soln.Assignments[0] == null || soln.Assignments[1] == null)
                return null;
            return soln.Assignments[0] == soln.Assignments[1];
        }

        [Test]
        [TestCase(100, 10, 40, 3, TestName = "10_Vars_Easy")] // 70ms
        [TestCase(100, 15, 65, 3, TestName = "15_Vars_Medium")] // 80ms
        [TestCase(100, 20, 80, 3, TestName = "20_Vars_Hard")] // 140ms
        [TestCase(100, 25, 110, 3, TestName = "25_Vars_VeryHard")] // 300ms
        //[TestCase(100, 50, 210, 3, TestName = "50_Vars_Extreme")] // 5.3s
        public void Random_EvenAssignments(int test_cases, int n_variables, int j_clauses, int k_literals)
        {
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new DpllTheorySolver<int>(EqualNumbersOfTrueAndFalseAssignment, vars);
            var solved = 0;
            for (var seed = 0; seed < test_cases; seed++)
            {
                var expression = GenerateExpression(seed, vars, j_clauses, k_literals);
                if (TestSolveTheory(sat, expression, EqualNumbersOfTrueAndFalseAssignment))
                    solved++;
            }
            var percent_solved = solved * 100 / test_cases;
            percent_solved.Should().BeInRange(40, 60);
        }

        /// <summary>An example theory test, which makes sure that the number of variables assigned to
        /// true is the same as the number of variables assigned to false. If the number of variables is
        /// odd, the last variable can be either.</summary>
        private bool? EqualNumbersOfTrueAndFalseAssignment(Solution soln)
        {
            int min = soln.Assignments.Length / 2;
            int max = (soln.Assignments.Length + 1) / 2;
            int count_true = 0;
            int count_false = 0;
            for (var i=0; i<soln.Assignments.Length; i++)
            {
                if (soln.Assignments[i] == true)
                    count_true++;
                else if (soln.Assignments[i] == false)
                    count_false++;
            }
            if (count_false > max || count_true > max)
                return false;
            if (count_false < min || count_true < min)
                return null;
            return true;
        }
    }
}

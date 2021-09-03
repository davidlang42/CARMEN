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
    public class ExpressionBuilderTests
    {
        [Test]
        public void AllVariablesEqual_HardCoded_2()
        {
            var expected_expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = true },
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var builder = new ExpressionBuilder(2, v => v[0] == v[1]);
            builder.Expression.ToString().Should().Be(expected_expression.ToString());
        }

        [Test]
        public void AllVariablesEqual_HardCoded_3()
        {
            var expected_expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = true },
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var builder = new ExpressionBuilder(3, values => values.All(v => v) || values.All(v => !v));
            builder.Expression.ToString().Should().Be(expected_expression.ToString());
        }

        [Test]
        public void HalfVariablesDifferent_HardCoded_2()
        {
            var expected_expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = false },
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var builder = new ExpressionBuilder(2, v => v[0] != v[1]);
            builder.Expression.ToString().Should().Be(expected_expression.ToString());
        }

        [Test]
        public void HalfVariablesDifferent_HardCoded_4()
        {
            var expected_expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = true },
                            new Literal<int> { Variable = 3, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = true },
                            new Literal<int> { Variable = 3, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = false },
                            new Literal<int> { Variable = 3, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = true },
                            new Literal<int> { Variable = 3, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = true },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = false },
                            new Literal<int> { Variable = 3, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = true },
                            new Literal<int> { Variable = 3, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = true },
                            new Literal<int> { Variable = 2, Polarity = false },
                            new Literal<int> { Variable = 3, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = true },
                            new Literal<int> { Variable = 3, Polarity = false },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = false },
                            new Literal<int> { Variable = 3, Polarity = true },
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            new Literal<int> { Variable = 0, Polarity = false },
                            new Literal<int> { Variable = 1, Polarity = false },
                            new Literal<int> { Variable = 2, Polarity = false },
                            new Literal<int> { Variable = 3, Polarity = false },
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var builder = new ExpressionBuilder(4, values => values.Count(v => v) == values.Count(v => !v));
            builder.Expression.ToString().Should().Be(expected_expression.ToString());
        }

        [Test]
        //[TestCase(5, false)] // 40ms
        //[TestCase(10, false)] // 50ms
        //[TestCase(15, false)] // 130ms
        //[TestCase(20, false)] // 3.5s
        //[TestCase(25, false)] // ?
        [TestCase(5)]
        [TestCase(10)]
        public void HalfVariablesDifferent_Evaluated(int n_vars, bool evaluate = true)
        {
            Func<bool[], bool> f = values => values.Count(v => v) == values.Count(v => !v);
            BuildAndTest(n_vars, f, evaluate);
        }

        [Test]
        //[TestCase(5, false)] // 40ms
        //[TestCase(10, false)] // 40ms
        //[TestCase(15, false)] // 130ms
        //[TestCase(20, false)] // 3.5s
        //[TestCase(25, false)] // ?
        [TestCase(5)]
        [TestCase(10)]
        public void AllVariablesEqual_Evaluated(int n_vars, bool evaluate = true)
        {
            Func<bool[], bool> f = values => values.Count(v => v) == values.Count(v => !v);
            BuildAndTest(n_vars, f, evaluate);
        }

        private void BuildAndTest(int n_vars, Func<bool[], bool> boolean_function, bool evaluate)
        {
            var vars = Enumerable.Range(0, n_vars).ToArray();
            var builder = new ExpressionBuilder(n_vars, boolean_function);
            if (!evaluate)
                Assert.Fail(); // abort if not evaluating, in order to give an accurate estimate of time taken to calculate expression only
            var solver = new AbstractSolver(vars);
            var combinations = new Solution { Assignments = new bool?[n_vars] }.Enumerate().ToArray();
            foreach (var combination in combinations)
            {
                var expected_result = boolean_function(combination.Assignments.Select(a => a!.Value).ToArray());
                solver.Evaluate(builder.Expression, combination).Should().Be(expected_result);
            }
        }
    }
}

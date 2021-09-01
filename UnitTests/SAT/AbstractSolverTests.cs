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
    public class AbstractSolverTests
    {
        [Test]
        public void Check_UnknownVariables_Invalid()
        {
            var var = 1;
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(var)
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
            sat.Introduce(expression);
            sat.Variables.Count.Should().Be(1);
            sat.Check(expression).Should().BeTrue();
        }

        [Test]
        public void Check_NoClauses_Invalid()
        {
            var expression = new Expression<int>();
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_DuplicateClauses_Invalid()
        {
            var var = 1;
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(var)
                        }.ToHashSet()
                    },
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(var)
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseNoLiterals_Invalid()
        {
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>()
                }.ToHashSet()
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseDuplicateLiterals_Invalid()
        {
            var var = 1;
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(var),
                            Literal<int>.Positive(var)
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseOppositeLiterals_Valid()
        {
            var var = 1;
            var expression = new Expression<int>
            {
                Clauses = new[]
                {
                    new Clause<int>
                    {
                        Literals = new[]
                        {
                            Literal<int>.Positive(var),
                            Literal<int>.Negative(var)
                        }.ToHashSet()
                    }
                }.ToHashSet()
            };
            var sat = new AbstractSolver(var);
            sat.Check(expression).Should().BeTrue();
        }
    }

    public class AbstractSolver : Solver<int>
    {
        public AbstractSolver(int variable)
            : this(new[] { variable })
        { }

        public AbstractSolver(IEnumerable<int>? variables = null)
            : base(variables)
        { }

        protected override IEnumerable<Solution> PartialSolve(Expression<int> expression, Solution partial_solution) => throw new NotImplementedException();
    }
}

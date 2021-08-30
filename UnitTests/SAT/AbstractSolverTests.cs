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
            var var = new NumberedVariable { Number = 1 };
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            var.PositiveLiteral
                        }
                    }
                }
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
            var expression = new Expression();
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_DuplicateClauses_Invalid()
        {
            var var = new NumberedVariable { Number = 1 };
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            var.PositiveLiteral
                        }
                    },
                    new Clause
                    {
                        Literals = new[]
                        {
                            var.PositiveLiteral
                        }
                    }
                }
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseNoLiterals_Invalid()
        {
            var var = new NumberedVariable { Number = 1 };
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause()
                }
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseDuplicateLiterals_Invalid()
        {
            var var = new NumberedVariable { Number = 1 };
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            var.PositiveLiteral,
                            var.PositiveLiteral
                        }
                    }
                }
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }

        [Test]
        public void Check_ClauseOppositeLiterals_Valid()
        {
            var var = new NumberedVariable { Number = 1 };
            var expression = new Expression
            {
                Clauses = new[]
                {
                    new Clause
                    {
                        Literals = new[]
                        {
                            var.PositiveLiteral,
                            var.NegativeLiteral
                        }
                    }
                }
            };
            var sat = new AbstractSolver();
            sat.Check(expression).Should().BeFalse();
        }
    }

    public class AbstractSolver : Solver
    {
        public AbstractSolver()
            : base(new())
        { }

        public override Solution? Solve(Expression expression) => throw new NotImplementedException();
    }
}

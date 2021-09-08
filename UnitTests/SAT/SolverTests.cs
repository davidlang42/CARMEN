﻿using Carmen.CastingEngine.SAT;
using Carmen.CastingEngine.SAT.Internal;
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
        protected bool TestSolve(Solver<int> sat, Expression<int> expression)
        {
            var solution = sat.Solve(expression).FirstOrDefault();
            if (!solution.IsUnsolvable)
            {
                sat.Evaluate(expression, solution.Enumerate().First()).Should().BeTrue();
                sat.Evaluate(expression, solution.Enumerate().Last()).Should().BeTrue();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TestSolveAll(Solver<int> sat, Expression<int> expression)
        {
            var solved = false;
            foreach(var solution in sat.Solve(expression))
            {
                solution.IsUnsolvable.Should().BeFalse();
                foreach (var full_assignment in solution.Enumerate())
                    sat.Evaluate(expression, full_assignment).Should().BeTrue();
                solved = true;
            }
            return solved;
        }

        protected bool TestSolveTheory(Solver<int> sat, Expression<int> expression, Func<Solution, bool?> validity_test)
        {
            var solution = sat.Solve(expression).FirstOrDefault();
            if (!solution.IsUnsolvable)
            {
                var first_filled = solution.Enumerate().First();
                sat.Evaluate(expression, first_filled).Should().BeTrue();
                validity_test(first_filled).Should().BeTrue();
                var last_filled = solution.Enumerate().Last();
                sat.Evaluate(expression, last_filled).Should().BeTrue();
                validity_test(last_filled).Should().BeTrue();
                return true;
            }
            else
            {
                return false;
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

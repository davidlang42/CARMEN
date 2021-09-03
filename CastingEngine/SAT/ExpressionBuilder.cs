﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// Calculate the boolean expression representing a given boolean function, using the truth table method
    /// </summary>
    public class ExpressionBuilder
    {
        private int nVariables;
        private Expression<int> expression;

        public ExpressionBuilder(int n_variables, Func<bool[], bool> boolean_function)
        {
            nVariables = n_variables;
            expression = TruthTableExpression(nVariables, boolean_function);
        }

        public Expression<T> Apply<T>(T[] variables)
            where T : notnull
        {
            if (variables.Length != nVariables)
                throw new ArgumentException($"Must provide exactly {nVariables} variables, as per constructor argument");
            int i = 0;
            var map = variables.ToDictionary(v => i++, v => v);
            return expression.Remap(map);
        }

        /// <summary>Enumerate all possible boolean values for the given array size, starting from a given index</summary>
        private static IEnumerable<bool[]> Enumerate(bool[] partial, int index)
        {
            partial = (bool[])partial.Clone();
            if (index == partial.Length)
                yield return partial;
            else
            {
                partial[index] = false;
                foreach (var full in Enumerate(partial, index + 1))
                    yield return full;
                partial[index] = true;
                foreach (var full in Enumerate(partial, index + 1))
                    yield return full;
            }
        }

        /// <summary>Calculate a CNF expression using the truth table method,
        /// that is add one clause per FALSE row in the truth table</summary>
        private static Expression<int> TruthTableExpression(int n_variables, Func<bool[], bool> boolean_function)
        {
            return new Expression<int>
            {
                Clauses = Enumerate(new bool[n_variables], 0)
                    .Where(values => !boolean_function(values))
                    .Select(values => TruthTableClause(values))
                    .ToHashSet()
            };
        }

        /// <summary>Calculate a CNF clause for the truth table method,
        /// that is one literal per variable, of the opposite polarity to the value</summary>
        private static Clause<int> TruthTableClause(bool[] values)
        {
            var literals = new HashSet<Literal<int>>();
            for (var i = 0; i < values.Length; i++)
                literals.Add(new Literal<int>
                {
                    Variable = i,
                    Polarity = !values[i]
                });
            return new Clause<int> { Literals = literals };
        }
    }
}

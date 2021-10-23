using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// Calculate the boolean expression representing a given boolean function, using the truth table method
    /// </summary>
    public class ExpressionBuilder
    {
        public int VariablesCount { get; init; }
        public Expression<int> Expression { get; init; }

        public ExpressionBuilder(int n_variables, Func<bool[], bool> boolean_function)
        {
            VariablesCount = n_variables;
            Expression = TruthTableExpression(n_variables, boolean_function);
        }

        public Expression<T> Apply<T>(T[] variables)
            where T : notnull
        {
            if (variables.Length != VariablesCount)
                throw new ArgumentException($"Must provide exactly {VariablesCount} variables, as per constructor argument");
            int i = 0;
            var map = variables.ToDictionary(v => i++, v => v);
            return Expression.Remap(map);
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
                literals.Add(new Literal<int>(i, !values[i]));
            return new Clause<int>(literals);
        }
    }
}

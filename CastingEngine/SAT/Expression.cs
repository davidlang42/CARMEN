using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A conjunction (AND) of boolean clauses.
    /// </summary>
    public struct Expression<T>
        where T : notnull
    {
        //LATER if speed is required, refactor the whole expression to be 3 dimensional matrix:
        // n variables => 2n literals
        // 1 clause => bool[2n] representing whether each literal is in the clause or not
        // 1 expression => j clauses => bool[j][2n] or bool[j][n][2]
        
        public const string CONJUNCTION = "∧";

        public HashSet<Clause<T>> Clauses { get; set; }

        public bool IsEmpty() => Clauses == null || Clauses.Count == 0;

        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));

        public Expression<U> Remap<U>(Dictionary<T, U> variable_map) where U : notnull
            => new()
            {
                Clauses = Clauses.Select(c => c.Remap(variable_map)).ToHashSet()
            };

        public Expression<T> Clone()
            => new()
            {
                Clauses = Clauses.Select(c => c.Clone()).ToHashSet()
            };
    }
}

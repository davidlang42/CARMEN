using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A conjunction (AND) of boolean clauses.
    /// </summary>
    public record Expression<T>
        where T : notnull
    {
        public const string CONJUNCTION = "∧";

        public HashSet<Clause<T>> Clauses;

        public Expression()
        {
            Clauses = new();
        }

        public Expression(HashSet<Clause<T>> clauses)
        {
            Clauses = clauses;
        }

        public bool IsEmpty() => Clauses == null || Clauses.Count == 0;

        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));

        public Expression<U> Remap<U>(Dictionary<T, U> variable_map) where U : notnull
            => new(Clauses.Select(c => c.Remap(variable_map)).ToHashSet());
    }
}

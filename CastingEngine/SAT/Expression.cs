using Carmen.CastingEngine.SAT.Internal;
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
        public const string CONJUNCTION = "∧";

        public HashSet<Clause<T>> Clauses { get; set; }

        public bool IsEmpty() => Clauses == null || Clauses.Count == 0;

        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));

        internal Expression Compress(IEnumerable<T> ordered_variables)
            => new()
            {
                Clauses = Clauses.Select(c => c.Compress(ordered_variables)).ToArray()
            };
    }
}

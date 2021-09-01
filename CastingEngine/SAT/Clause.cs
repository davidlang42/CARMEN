using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A disjunction (OR) of boolean literals.
    /// </summary>
    public struct Clause<T>
        where T : notnull
    {
        public const string DISJUNCTION = "∨";

        public HashSet<Literal<T>> Literals { get; set; }

        public bool IsEmpty() => Literals == null || Literals.Count == 0;

        public bool IsUnitClause(out Literal<T> single_literal)
        {
            if (Literals == null || Literals.Count == 0 || Literals.Count > 1)
            {
                single_literal = default;
                return false;
            }
            else
            {
                single_literal = Literals.First();
                return true;
            }
        }

        public override string ToString() => string.Join(DISJUNCTION, Literals.Select(l => l.ToString()));

        public Clause<U> Remap<U>(Dictionary<T, U> variable_map) where U : notnull
            => new()
            {
                Literals = Literals.Select(l => l.Remap(variable_map)).ToHashSet()
            };

        public Clause<T> Clone()
            => new()
            {
                Literals = Literals.Select(l => l.Clone()).ToHashSet()
            };
    }
}

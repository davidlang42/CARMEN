using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A disjunction (OR) of boolean literals.
    /// </summary>
    public record Clause<T>
        where T : notnull
    {
        public const string DISJUNCTION = "∨";

        public HashSet<Literal<T>> Literals;

        public Clause()
        {
            Literals = new();
        }

        public Clause(HashSet<Literal<T>> literals)
        {
            Literals = literals;
        }

        public bool IsEmpty() => Literals == null || Literals.Count == 0;

        public bool IsUnitClause([NotNullWhen(true)]out Literal<T>? single_literal)
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
            => new(Literals.Select(l => l.Remap(variable_map)).ToHashSet());

        public static Clause<T> Unit(T variable, bool polarity)
            => Unit(new Literal<T>(variable, polarity));

        public static Clause<T> Unit(Literal<T> literal)
            => new(literal.Yield().ToHashSet());
    }
}

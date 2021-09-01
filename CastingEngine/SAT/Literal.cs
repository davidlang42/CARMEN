using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A positive or negative literal of a boolean variable.
    /// </summary>
    public struct Literal<T>
        where T : notnull
    {
        public T Variable { get; set; }
        public bool Polarity { get; set; }

        public Literal<T> Inverse() => new Literal<T> { Variable = Variable, Polarity = !Polarity };

        public static Literal<T> Positive(T variable) => new Literal<T> { Variable = variable, Polarity = true };

        public static Literal<T> Negative(T variable) => new Literal<T> { Variable = variable, Polarity = false };

        public override string ToString() => Polarity ? VariableName : $"neg({VariableName})";

        public string VariableName => Variable switch
            {
                int i => $"X{i}",
                string s => s,
                _ => Variable.ToString() ?? "?"
            };

        public Literal<U> Remap<U>(Dictionary<T, U> variable_map) where U : notnull
        {
            if (!variable_map.TryGetValue(Variable, out var mapped_variable))
                throw new ArgumentException($"{nameof(variable_map)} did not contain variable '{VariableName}'");
            return new()
            {
                Variable = mapped_variable,
                Polarity = Polarity
            };
        }

        public Literal<T> Clone() => new Literal<T> { Variable = Variable, Polarity = Polarity };

        public override bool Equals(object? obj)
            => obj is Literal<T> other && other.Polarity == Polarity && other.Variable.Equals(Variable);

        public override int GetHashCode() => Variable.GetHashCode() ^ Polarity.GetHashCode();
    }
}

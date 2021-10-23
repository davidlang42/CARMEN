using System;
using System.Collections.Generic;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A positive or negative literal of a boolean variable.
    /// </summary>
    public record Literal<T>
        where T : notnull
    {
        public T Variable;
        public bool Polarity;

        public Literal(T variable, bool polarity)
        {
            Variable = variable;
            Polarity = polarity;
        }

        public Literal<T> Inverse() => this with { Polarity = !Polarity };

        public static Literal<T> Positive(T variable) => new Literal<T>(variable, true);

        public static Literal<T> Negative(T variable) => new Literal<T>(variable, false);

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
            return new(mapped_variable, Polarity);
        }
    }
}

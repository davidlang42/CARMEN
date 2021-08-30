using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// An object representing a unique boolean variable, excluding state.
    /// </summary>
    public abstract class Variable
    {
        public Literal PositiveLiteral => new Literal { Variable = this, Inverse = false };
        public Literal NegativeLiteral => new Literal { Variable = this, Inverse = true };

        public abstract override string ToString();
    }

    public class NumberedVariable : Variable
    {
        public uint Number { get; set; }

        public override string ToString() => $"X{Number}";
    }

    public class NamedVariable : Variable
    {
        public string Name { get; set; } = "";

        public override string ToString() => Name;
    }

    public class ObjectVariable<T> : Variable
        where T : class
    {
        public T Object { get; init; }

        public ObjectVariable(T obj)
        {
            Object = obj;
        }

        public override string ToString() => $"VAR:{Object.ToString() ?? "?"}";
    }
}

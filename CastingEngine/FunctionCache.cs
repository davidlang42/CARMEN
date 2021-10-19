using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    public class FunctionCache<T, U>
        where T : notnull
    {
        readonly Dictionary<T, U> cache = new();

        public Func<T, U>? Function { get; set; }

        public FunctionCache(Func<T, U>? function = null)
        {
            Function = function;
        }

        public U Get(T input, Func<T, U>? function_override = null)
        {
            if (!cache.TryGetValue(input, out var value))
            {
                var function = Function ?? function_override ?? throw new ArgumentException("Function not set");
                value = function(input);
                cache.Add(input, value);
            }
            return value;
        }

        public U this[T input] => Get(input);
    }

    public class FunctionCache<T1, T2, U>
    {
        readonly Dictionary<(T1, T2), U> cache = new();

        public Func<T1, T2, U>? Function { get; set; }

        public FunctionCache(Func<T1, T2, U>? function = null)
        {
            Function = function;
        }

        public U Get(T1 input1, T2 input2, Func<T1, T2, U>? function_override = null)
        {
            if (!cache.TryGetValue((input1, input2), out var value))
            {
                var function = Function ?? function_override ?? throw new ArgumentException("Function not set");
                value = function(input1, input2);
                cache.Add((input1, input2), value);
            }
            return value;
        }

        public U this[(T1, T2) input] => Get(input.Item1, input.Item2);
    }
}

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
        readonly Func<T, U> calculate;
        readonly Dictionary<T, U> cache = new();

        public FunctionCache(Func<T, U> calculate)
        {
            this.calculate = calculate;
        }

        public U Get(T input)
        {
            if (!cache.TryGetValue(input, out var value))
            {
                value = calculate(input);
                cache.Add(input, value);
            }
            return value;
        }

        public U this[T input] => Get(input);
    }

    public class FunctionCache<T1, T2, U>
    {
        readonly Func<T1, T2, U> calculate;
        readonly Dictionary<(T1, T2), U> cache = new();

        public FunctionCache(Func<T1, T2, U> calculate)
        {
            this.calculate = calculate;
        }

        public U Get(T1 input1, T2 input2)
        {
            if (!cache.TryGetValue((input1, input2), out var value))
            {
                value = calculate(input1, input2);
                cache.Add((input1, input2), value);
            }
            return value;
        }

        public U this[(T1, T2) input] => Get(input.Item1, input.Item2);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.Properties
{
    /// <summary>
    /// A Dictionary which is all-knowing (of keys) when indexed.
    /// </summary>
    public class OmnificentDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TKey : notnull where TValue : struct
    {
        public TValue DefaultValue { get; set; } = default;

        public new TValue this[TKey key]
        {
            get => TryGetValue(key, out var value) ? value : DefaultValue;
            set => base[key] = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.Properties
{
    /// <summary>
    /// A wrapper which makes a Dictionary all-knowing (of keys).
    /// </summary>
    public class OmnificentDictionaryWrapper<TKey, TValue>
        where TKey : notnull
    {
        IDictionary<TKey, TValue> dictionary;
        TValue defaultValue;

        public OmnificentDictionaryWrapper(IDictionary<TKey, TValue> dictionary, TValue default_value)
        {
            this.dictionary = dictionary;
            this.defaultValue = default_value;
        }

        public TValue this[TKey key]
        {
            get => dictionary.TryGetValue(key, out var value) ? value : defaultValue;
            set => dictionary[key] = value;
        }
    }
}

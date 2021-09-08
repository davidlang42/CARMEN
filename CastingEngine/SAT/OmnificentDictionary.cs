using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A Dictionary which is all-knowing (of keys) when accessed via the indexer.
    /// </summary>
    public class OmnificentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : struct
    {
        Dictionary<TKey, TValue> dictionary = new();

        public TValue this[TKey key]
        {
            get => dictionary.TryGetValue(key, out var value) ? value : default;
            set => dictionary[key] = value;
        }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)dictionary).Keys;
        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)dictionary).Values;
        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;
        public void Add(TKey key, TValue value) => ((IDictionary<TKey, TValue>)dictionary).Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(item);
        public void Clear() => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue>)dictionary).ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        public bool Remove(TKey key) => ((IDictionary<TKey, TValue>)dictionary).Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(item);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => ((IDictionary<TKey, TValue>)dictionary).TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Collections
{
    internal class FilteredSortedCollection<T> : ICollection<T>, INotifyCollectionChanged
    {
        //TODO actually filter
        //TODO actually sort
        readonly List<T> items;

        public FilteredSortedCollection(IEnumerable<T> items)
        {
            this.items = new List<T>(items);
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            items.Add(item);
            OnCollectionChanged();
        }

        public bool Remove(T item)
        {
            var removed = items.Remove(item);
            OnCollectionChanged();
            return removed;
        }

        protected void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        #region Not implemented
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        #endregion
    }
}

using Carmen.ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Collections
{
    internal class FilteredSortedCollection<T> : ICollection<T>, INotifyCollectionChanged where T : INotifyPropertyChanged
    {
        readonly List<T> items;

        private IComparer<T>? sortBy = null;
        public IComparer<T> SortBy
        {
            set
            {
                if (sortBy == value)
                    return;
                sortBy = value;
                items.Sort(sortBy);
                OnCollectionChanged();
            }
        }

        private Func<T, bool>? filter = null;
        public Func<T, bool>? Filter
        {
            get => filter;
            set
            {
                if (filter == value)
                    return;
                filter = value;
                OnCollectionChanged();
            }
        }

        public FilteredSortedCollection(IEnumerable<T> items)
        {
            this.items = new List<T>(items);
            foreach (var item in items)
                item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sortBy == null)
                return; // if we aren't sorted, it doesn't matter
            if (sender is T changed_item && items.Remove(changed_item))
            {
                // if we can find it, we can move it
                InsertItemInOrder(items, changed_item, sortBy);
            }
            else
            {
                // if we can't, then resort everything to be safe
                items.Sort(sortBy);
            }
            OnCollectionChanged();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public IEnumerator<T> GetEnumerator() => (filter == null ? items : items.Where(filter)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            item.PropertyChanged += Item_PropertyChanged;
            if (sortBy == null)
                items.Add(item);
            else
                InsertItemInOrder(items, item, sortBy);
            OnCollectionChanged();
        }

        public bool Remove(T item)
        {
            item.PropertyChanged -= Item_PropertyChanged;
            var removed = items.Remove(item);
            OnCollectionChanged();
            return removed;
        }

        protected void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        private static void InsertItemInOrder(List<T> items, T new_item, IComparer<T> comparer)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (comparer.Compare(items[i], new_item) > 0)
                {
                    items.Insert(i, new_item);
                    return;
                }
            }
            items.Add(new_item);
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

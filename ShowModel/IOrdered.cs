using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.ShowModel
{
    /// <summary>
    /// An object which can be ordered when in a collection
    /// </summary>
    public interface IOrdered : IComparable
    {
        int Order { get; set; }

        int IComparable.CompareTo(object? obj) => Order.CompareTo(((IOrdered?)obj)?.Order);
    }

    public static class IOrderedExtensions
    {
        /// <summary>Enumerates this collection by Order value.</summary>
        public static IOrderedEnumerable<T> InOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.OrderBy(o => o.Order);

        /// <summary>Finds the first Order value of this collection.</summary>
        public static int FirstOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.Select(o => o.Order).DefaultIfEmpty().Min();

        /// <summary>Finds the Order value which would place a new object at the end of this collection.</summary>
        public static int NextOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.Select(o => o.Order).DefaultIfEmpty().Max() + 1;

        /// <summary>Updates the Order value of objects in this collection to make space for the new object
        /// after the provided object, and adds the new object. If after==null, object is placed at the end.</summary>
        public static void InsertInOrder<T>(this ICollection<T> objects, T add_object, T? after = null) where T : class, IOrdered
            => InsertInOrder(new CollectionWrapper<T>(objects), add_object, after);

        /// <summary>Updates the Order value of objects in this collection to make space for the new object
        /// after the provided object, and adds the new object. If after==null, object is placed at the end.</summary>
        public static void InsertInOrder<T>(this DbSet<T> objects, T add_object, T? after = null) where T : class, IOrdered
            => InsertInOrder(new DbSetWrapper<T>(objects), add_object, after);

        private static void InsertInOrder<T>(this IDbSetOrCollection<T> objects, T add_object, T? after = null) where T : class, IOrdered
        {
            if (after != null)
            {
                if (!objects.Contains(after))
                    throw new ArgumentException($"'{nameof(after)}' must be in this collection.");
                var new_order = after.Order + 1;
                var objects_after = objects.Where(o => o.Order >= new_order).ToDictionary(o => o.Order);
                var shift_order = new_order;
                T? shift_object;
                while (objects_after.TryGetValue(shift_order, out shift_object))
                {
                    shift_object.Order++;
                    shift_order++;
                }
                add_object.Order = new_order;
            }
            else
            {
                add_object.Order = objects.NextOrder();
            }
            objects.Add(add_object);
        }

        /// <summary>Updates the Order value of objects in this collection to move an object to
        /// after another object.</summary>
        public static void MoveInOrder<T>(this ICollection<T> objects, T move_object, T after) where T : class, IOrdered
            => MoveInOrder(new CollectionWrapper<T>(objects), move_object, after);

        /// <summary>Updates the Order value of objects in this collection to move an object to
        /// after another object.</summary>
        public static void MoveInOrder<T>(this DbSet<T> objects, T move_object, T after) where T : class, IOrdered
            => MoveInOrder(new DbSetWrapper<T>(objects), move_object, after);

        private static void MoveInOrder<T>(this IDbSetOrCollection<T> objects, T move_object, T after) where T : class, IOrdered
        {
            if (!objects.Contains(after))
                throw new ArgumentException($"'{nameof(after)}' must be in this collection.");
            var new_order = move_object.Order < after.Order ? after.Order : after.Order + 1;
            MoveInOrder(objects, move_object, new_order);
        }

        /// <summary>Updates the Order value of objects in this collection to move an object to
        /// a new order value.</summary>
        public static void MoveInOrder<T>(this ICollection<T> objects, T move_object, int new_order) where T : class, IOrdered
            => MoveInOrder(new CollectionWrapper<T>(objects), move_object, new_order);

        /// <summary>Updates the Order value of objects in this collection to move an object to
        /// a new order value.</summary>
        public static void MoveInOrder<T>(this DbSet<T> objects, T move_object, int new_order) where T : class, IOrdered
            => MoveInOrder(new DbSetWrapper<T>(objects), move_object, new_order);

        private static void MoveInOrder<T>(this IDbSetOrCollection<T> objects, T move_object, int new_order) where T : class, IOrdered
        {
            if (!objects.Contains(move_object))
                throw new ArgumentException($"'{nameof(move_object)}' must be in this collection.");
            var old_order = move_object.Order;   
            if (new_order == old_order || new_order == old_order + 1)
                return; // no change
            Dictionary<int, T> objects_between;
            int delta;
            if (old_order < new_order)
            {
                objects_between = objects.Where(o => o.Order > old_order && o.Order <= new_order).ToDictionary(o => o.Order);
                delta = -1;
            }
            else
            {
                objects_between = objects.Where(o => o.Order >= new_order && o.Order < old_order).ToDictionary(o => o.Order);
                delta = 1;
            }
            var shift_order = new_order;
            T? shift_object;
            while (objects_between.TryGetValue(shift_order, out shift_object))
            {
                shift_object.Order += delta;
                shift_order += delta;
            }
            move_object.Order = new_order;
        }

        private interface IDbSetOrCollection<T> : IEnumerable<T>
        {
            public void Add(T item);
        }

        private class DbSetWrapper<T> : IDbSetOrCollection<T> where T : class
        {
            private DbSet<T> dbSet;

            public DbSetWrapper(DbSet<T> dbSet) => this.dbSet = dbSet;

            public void Add(T item) => dbSet.Add(item);
            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)dbSet).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class CollectionWrapper<T> : IDbSetOrCollection<T> where T : class
        {
            private ICollection<T> collection;

            public CollectionWrapper(ICollection<T> collection) => this.collection = collection;

            public void Add(T item) => collection.Add(item);
            public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

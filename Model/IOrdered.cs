using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    /// <summary>
    /// An object which can be ordered when in a collection
    /// </summary>
    public interface IOrdered
    {
        int Order { get; set;  }
    }

    public static class IOrderedExtensions
    {
        /// <summary>Enumerates this collection by Order value.</summary>
        public static IOrderedEnumerable<T> InOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.OrderBy(o => o.Order);

        /// <summary>Finds the Order value which would place a new object at the end of this collection.</summary>
        public static int NextOrder<T>(this IEnumerable<T> objects) where T : IOrdered
            => objects.Select(o => o.Order).DefaultIfEmpty().Max() + 1;

        /// <summary>Updates the Order value of objects in this collection to make space for the new object
        /// after the provided object, and adds the new object. If after==null, object is placed at the end.</summary>
        public static void InsertInOrder<T>(this ICollection<T> objects, T add_object, T? after = null) where T : class, IOrdered
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
        {
            if (!objects.Contains(after))
                throw new ArgumentException($"'{nameof(after)}' must be in this collection.");
            if (!objects.Contains(move_object))
                throw new ArgumentException($"'{nameof(move_object)}' must be in this collection.");
            var old_order = move_object.Order;
            var new_order = after.Order + 1;
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
    }
}

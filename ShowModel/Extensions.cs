using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ShowModel
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static uint Sum(this IEnumerable<uint> list)
            => (uint)list.Cast<int>().Sum();

        public static double Product(this IEnumerable<double> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("Cannot calculate product of an empty sequence.");
            var result = e.Current;
            while (e.MoveNext())
                result *= e.Current;
            return result;
        }

        /// <summary>Similar to SingleOrDefault(), except if the sequence contains more than 1 element,
        /// the default is returned rather than throwing an exception.</summary>
        public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> list)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
                return default;
            var first = e.Current;
            if (e.MoveNext())
                return default;
            return first;
        }

        public static T Random<T>(this IList<T> list, Random? random = null)
            => list[(random ?? new Random()).Next(list.Count)];
    }

    internal static class EntityExtensions
    {
        /// <summary>Use the property name as the column name, even if that means the column is shared with another property.
        /// This is useful when 2 inherited classes of the same base have a common property.</summary>
        public static void CommonProperty<T>(this EntityTypeBuilder<T> entity, string property) where T : class
            => entity.Property(property).HasColumnName(property);

        /// <summary>Configure the foreign key back to the owner of the Owned entity, and also configure the key
        /// of the Owned entity as a composite key { owner_key, another_field }.</summary>
        public static void WithOwnerCompositeKey<T, U>(this OwnedNavigationBuilder<T, U> builder, string owner_key, string composite_with_owner_key)
            where T : class
            where U : class
        {
            builder.WithOwner().HasForeignKey(owner_key);
            builder.HasKey(owner_key, composite_with_owner_key);
        }
    }
}

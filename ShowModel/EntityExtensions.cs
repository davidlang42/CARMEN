using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carmen.ShowModel
{
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

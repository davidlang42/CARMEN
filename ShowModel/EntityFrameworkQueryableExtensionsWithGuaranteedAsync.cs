using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Carmen.ShowModel
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityFrameworkQueryableExtensionsWithGuaranteedAsync
    {
        //
        // Summary:
        //     Specifies related entities to include in the query results. The navigation property
        //     to be included is specified starting with the type of entity being queried (TEntity).
        //     Further navigation properties to be included can be appended, separated by the
        //     '.' character.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   navigationPropertyPath:
        //     A string of '.' separated navigation property names to be included.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query with the related data included.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or navigationPropertyPath is null.
        //
        //   T:System.ArgumentException:
        //     navigationPropertyPath is empty or whitespace.
        public static IQueryable<TEntity> Include<TEntity>([NotNullAttribute] this IQueryable<TEntity> source, [NotNullAttribute][NotParameterized] string navigationPropertyPath) where TEntity : class
            => EntityFrameworkQueryableExtensions.Include(source, navigationPropertyPath);
        //
        // Summary:
        //     Specifies related entities to include in the query results. The navigation property
        //     to be included is specified starting with the type of entity being queried (TEntity).
        //     If you wish to include additional types based on the navigation properties of
        //     the type being included, then chain a call to Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ThenInclude``3(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable{``0,System.Collections.Generic.IEnumerable{``1}},System.Linq.Expressions.Expression{System.Func{``1,``2}})
        //     after this call.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   navigationPropertyPath:
        //     A lambda expression representing the navigation property to be included (t =>
        //     t.Property1).
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        //   TProperty:
        //     The type of the related entity to be included.
        //
        // Returns:
        //     A new query with the related data included.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or navigationPropertyPath is null.
        public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>([NotNullAttribute] this IQueryable<TEntity> source, [NotNullAttribute] Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TEntity : class
            => EntityFrameworkQueryableExtensions.Include(source, navigationPropertyPath);
        //
        // Summary:
        //     Asynchronously returns the only element of a sequence, and throws an exception
        //     if there is not exactly one element in the sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the single element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     single element of the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains more than one elements.
        //     -or-
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> SingleAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default)
            => Task.Run(() => source.Single(), cancellationToken);
        //
        // Summary:
        //     Asynchronously creates an array from an System.Linq.IQueryable`1 by enumerating
        //     it asynchronously.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create an array from.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains an
        //     array that contains elements from the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource[]> ToArrayAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default)
            => Task.Run(() => source.ToArray(), cancellationToken);
        //
        // Summary:
        //     Enumerates the query. When using Entity Framework, this causes the results of
        //     the query to be loaded into the associated context. This is equivalent to calling
        //     ToList and then throwing away the list (without the overhead of actually creating
        //     the list).
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static void Load<TSource>([NotNullAttribute] this IQueryable<TSource> source)
            => EntityFrameworkQueryableExtensions.Load(source);
        //
        // Summary:
        //     Asynchronously enumerates the query. When using Entity Framework, this causes
        //     the results of the query to be loaded into the associated context. This is equivalent
        //     to calling ToList and then throwing away the list (without the overhead of actually
        //     creating the list).
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static Task LoadAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default)
            => Task.Run(() => EntityFrameworkQueryableExtensions.Load(source), cancellationToken);
        //
        // Summary:
        //     Specifies additional related data to be further included based on a related type
        //     that was just included.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   navigationPropertyPath:
        //     A lambda expression representing the navigation property to be included (t =>
        //     t.Property1).
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        //   TPreviousProperty:
        //     The type of the entity that was just included.
        //
        //   TProperty:
        //     The type of the related entity to be included.
        //
        // Returns:
        //     A new query with the related data included.
        public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>([NotNullAttribute] this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> source, [NotNullAttribute] Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath) where TEntity : class
            => EntityFrameworkQueryableExtensions.ThenInclude(source, navigationPropertyPath);
        //
        // Summary:
        //     Specifies additional related data to be further included based on a related type
        //     that was just included.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   navigationPropertyPath:
        //     A lambda expression representing the navigation property to be included (t =>
        //     t.Property1).
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        //   TPreviousProperty:
        //     The type of the entity that was just included.
        //
        //   TProperty:
        //     The type of the related entity to be included.
        //
        // Returns:
        //     A new query with the related data included.
        public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>([NotNullAttribute] this IIncludableQueryable<TEntity, TPreviousProperty> source, [NotNullAttribute] Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath) where TEntity : class
            => EntityFrameworkQueryableExtensions.ThenInclude(source, navigationPropertyPath);

        #region Not yet implemented
        /*
        //
        // Summary:
        //     Asynchronously returns the last element of a sequence, or a default value if
        //     the sequence contains no elements.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the last element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains default
        //     ( TSource ) if source is empty; otherwise, the last element in source.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> LastOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously determines whether all the elements of a sequence satisfy a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 whose elements to test for a condition.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains true
        //     if every element of the source sequence passes the test in the specified predicate;
        //     otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<bool> AllAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously determines whether a sequence contains any elements.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to check for being empty.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains true
        //     if the source sequence contains any elements; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<bool> AnyAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously determines whether any element of a sequence satisfies a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 whose elements to test for a condition.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains true
        //     if any elements in the source sequence pass the test in the specified predicate;
        //     otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<bool> AnyAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Returns an System.Collections.Generic.IAsyncEnumerable`1 which can be enumerated
        //     asynchronously.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to enumerate.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     The query results.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     source is null.
        //
        //   T:System.ArgumentNullException:
        //     source is not a System.Collections.Generic.IAsyncEnumerable`1.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>([NotNullAttribute] this IQueryable<TSource> source);
        //
        // Summary:
        //     The change tracker will not track any of the entities that are returned from
        //     a LINQ query. If the entity instances are modified, this will not be detected
        //     by the change tracker and Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     will not persist those changes to the database.
        //     Disabling change tracking is useful for read-only scenarios because it avoids
        //     the overhead of setting up change tracking for each entity instance. You should
        //     not disable change tracking if you want to manipulate entity instances and persist
        //     those changes to the database using Microsoft.EntityFrameworkCore.DbContext.SaveChanges.
        //     Identity resolution will not be performed. If an entity with a given key is in
        //     different result in the result set then they will be different instances.
        //     The default tracking behavior for queries can be controlled by Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.QueryTrackingBehavior.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query where the result set will not be tracked by the context.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static IQueryable<TEntity> AsNoTracking<TEntity>([NotNullAttribute] this IQueryable<TEntity> source) where TEntity : class;
        //
        // Summary:
        //     The change tracker will not track any of the entities that are returned from
        //     a LINQ query. If the entity instances are modified, this will not be detected
        //     by the change tracker and Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     will not persist those changes to the database.
        //     Disabling change tracking is useful for read-only scenarios because it avoids
        //     the overhead of setting up change tracking for each entity instance. You should
        //     not disable change tracking if you want to manipulate entity instances and persist
        //     those changes to the database using Microsoft.EntityFrameworkCore.DbContext.SaveChanges.
        //     Identity resolution will be performed to ensure that all occurrences of an entity
        //     with a given key in the result set are represented by the same entity instance.
        //     The default tracking behavior for queries can be controlled by Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.QueryTrackingBehavior.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query where the result set will not be tracked by the context.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static IQueryable<TEntity> AsNoTrackingWithIdentityResolution<TEntity>([NotNullAttribute] this IQueryable<TEntity> source) where TEntity : class;
        //
        // Summary:
        //     Returns a new query where the change tracker will either keep track of changes
        //     or not for all entities that are returned, depending on the value of the 'track'
        //     parameter. When tracking, Any modification to the entity instances will be detected
        //     and persisted to the database during Microsoft.EntityFrameworkCore.DbContext.SaveChanges.
        //     When not tracking, if the entity instances are modified, this will not be detected
        //     by the change tracker and Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     will not persist those changes to the database.
        //     Disabling change tracking is useful for read-only scenarios because it avoids
        //     the overhead of setting up change tracking for each entity instance. You should
        //     not disable change tracking if you want to manipulate entity instances and persist
        //     those changes to the database using Microsoft.EntityFrameworkCore.DbContext.SaveChanges.
        //     The default tracking behavior for queries can be controlled by Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.QueryTrackingBehavior.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   track:
        //     Indicates whether the query will track results or not.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query where the result set will be tracked by the context.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static IQueryable<TEntity> AsTracking<TEntity>([NotNullAttribute] this IQueryable<TEntity> source, QueryTrackingBehavior track) where TEntity : class;
        //
        // Summary:
        //     Returns a new query where the change tracker will keep track of changes for all
        //     entities that are returned. Any modification to the entity instances will be
        //     detected and persisted to the database during Microsoft.EntityFrameworkCore.DbContext.SaveChanges.
        //     The default tracking behavior for queries can be controlled by Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.QueryTrackingBehavior.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query where the result set will be tracked by the context.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static IQueryable<TEntity> AsTracking<TEntity>([NotNullAttribute] this IQueryable<TEntity> source) where TEntity : class;
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float?> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float?> AverageAsync([NotNullAttribute] this IQueryable<float?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float> AverageAsync([NotNullAttribute] this IQueryable<float> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync([NotNullAttribute] this IQueryable<double?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal> AverageAsync([NotNullAttribute] this IQueryable<decimal> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync([NotNullAttribute] this IQueryable<double> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal?> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync([NotNullAttribute] this IQueryable<int> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync([NotNullAttribute] this IQueryable<int?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values that is obtained
        //     by invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the projected values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> AverageAsync([NotNullAttribute] this IQueryable<long> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> AverageAsync([NotNullAttribute] this IQueryable<long?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the average of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the average of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     average of the sequence of values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal?> AverageAsync([NotNullAttribute] this IQueryable<decimal?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously determines whether a sequence contains a specified element by
        //     using the default equality comparer.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the single element of.
        //
        //   item:
        //     The object to locate in the sequence.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains true
        //     if the input sequence contains the specified value; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<bool> ContainsAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] TSource item, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to be counted.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     number of elements in the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int> CountAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence that satisfy a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     number of elements in the sequence that satisfy the condition in the predicate
        //     function.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int> CountAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the first element of a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the first element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     first element in source.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> FirstAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the first element of a sequence that satisfies a specified
        //     condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the first element of.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     first element in source that passes the test in predicate.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        //   T:System.InvalidOperationException:
        //     No element satisfies the condition in predicate
        //     -or -
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> FirstAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the first element of a sequence, or a default value if
        //     the sequence contains no elements.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the first element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains default
        //     ( TSource ) if source is empty; otherwise, the first element in source.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> FirstOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the first element of a sequence that satisfies a specified
        //     condition or a default value if no such element is found.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the first element of.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains default
        //     ( TSource ) if source is empty or if no element passes the test specified by
        //     predicate ; otherwise, the first element in source that passes the test specified
        //     by predicate.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> FirstOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously enumerates the query results and performs the specified action
        //     on each element.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to enumerate.
        //
        //   action:
        //     The action to perform on each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   T:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or action is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        [AsyncStateMachine(typeof(EntityFrameworkQueryableExtensions.< ForEachAsync > d__97<>))]
        public static Task ForEachAsync<T>([NotNullAttribute] this IQueryable<T> source, [NotNullAttribute] Action<T> action, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Specifies that the current Entity Framework LINQ query should not have any model-level
        //     eager loaded navigations applied.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query that will not apply any model-level eager loaded navigations.
        public static IQueryable<TEntity> IgnoreAutoIncludes<TEntity>([NotNullAttribute] this IQueryable<TEntity> source) where TEntity : class;
        //
        // Summary:
        //     Specifies that the current Entity Framework LINQ query should not have any model-level
        //     entity query filters applied.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query that will not apply any model-level entity query filters.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>([NotNullAttribute] this IQueryable<TEntity> source) where TEntity : class;
        //
        // Summary:
        //     Asynchronously returns the last element of a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the last element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     last element in source.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> LastAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the last element of a sequence that satisfies a specified
        //     condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the last element of.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     last element in source that passes the test in predicate.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        //   T:System.InvalidOperationException:
        //     No element satisfies the condition in predicate.
        //     -or-
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> LastAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the last element of a sequence that satisfies a specified
        //     condition or a default value if no such element is found.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the last element of.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains default
        //     ( TSource ) if source is empty or if no element passes the test specified by
        //     predicate ; otherwise, the last element in source that passes the test specified
        //     by predicate.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> LastOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns an System.Int64 that represents the total number of elements
        //     in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to be counted.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     number of elements in the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long> LongCountAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns an System.Int64 that represents the number of elements
        //     in a sequence that satisfy a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     number of elements in the sequence that satisfy the condition in the predicate
        //     function.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long> LongCountAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the maximum value of a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to determine the maximum
        //     of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     maximum value in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> MaxAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously invokes a projection function on each element of a sequence and
        //     returns the maximum resulting value.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to determine the maximum
        //     of.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TResult:
        //     The type of the value returned by the function represented by selector .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     maximum value in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TResult> MaxAsync<TSource, TResult>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the minimum value of a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to determine the minimum
        //     of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     minimum value in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> MinAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously invokes a projection function on each element of a sequence and
        //     returns the minimum resulting value.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 that contains the elements to determine the minimum
        //     of.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TResult:
        //     The type of the value returned by the function represented by selector .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     minimum value in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TResult> MinAsync<TSource, TResult>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the only element of a sequence that satisfies a specified
        //     condition, and throws an exception if more than one such element exists.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the single element of.
        //
        //   predicate:
        //     A function to test an element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     single element of the input sequence that satisfies the condition in predicate.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        //   T:System.InvalidOperationException:
        //     No element satisfies the condition in predicate.
        //     -or-
        //     More than one element satisfies the condition in predicate.
        //     -or-
        //     source contains no elements.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> SingleAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the only element of a sequence, or a default value if
        //     the sequence is empty; this method throws an exception if there is more than
        //     one element in the sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the single element of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     single element of the input sequence, or default ( TSource) if the sequence contains
        //     no elements.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidOperationException:
        //     source contains more than one element.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> SingleOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously returns the only element of a sequence that satisfies a specified
        //     condition or a default value if no such element exists; this method throws an
        //     exception if more than one element satisfies the condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to return the single element of.
        //
        //   predicate:
        //     A function to test an element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     single element of the input sequence that satisfies the condition in predicate,
        //     or default ( TSource ) if no such element is found.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or predicate is null.
        //
        //   T:System.InvalidOperationException:
        //     More than one element satisfies the condition in predicate.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<TSource> SingleOrDefaultAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float?> SumAsync([NotNullAttribute] this IQueryable<float?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal> SumAsync([NotNullAttribute] this IQueryable<decimal> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal?> SumAsync([NotNullAttribute] this IQueryable<decimal?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<decimal?> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int> SumAsync([NotNullAttribute] this IQueryable<int> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int?> SumAsync([NotNullAttribute] this IQueryable<int?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float?> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long> SumAsync([NotNullAttribute] this IQueryable<long> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<int?> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<float> SumAsync([NotNullAttribute] this IQueryable<float> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of the sequence of values that is obtained by
        //     invoking a projection function on each element of the input sequence.
        //
        // Parameters:
        //   source:
        //     A sequence of values of type TSource.
        //
        //   selector:
        //     A projection function to apply to each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the projected values..
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or selector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long?> SumAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double> SumAsync([NotNullAttribute] this IQueryable<double> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<double?> SumAsync([NotNullAttribute] this IQueryable<double?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously computes the sum of a sequence of values.
        //
        // Parameters:
        //   source:
        //     A sequence of values to calculate the sum of.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains the
        //     sum of the values in the sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<long?> SumAsync([NotNullAttribute] this IQueryable<long?> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Adds a tag to the collection of tags associated with an EF LINQ query. Tags are
        //     query annotations that can provide contextual tracing information at different
        //     points in the query pipeline.
        //
        // Parameters:
        //   source:
        //     The source query.
        //
        //   tag:
        //     The tag.
        //
        // Type parameters:
        //   T:
        //     The type of entity being queried.
        //
        // Returns:
        //     A new query annotated with the given tag.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or tag is null.
        //
        //   T:System.ArgumentException:
        //     tag is empty or whitespace.
        public static IQueryable<T> TagWith<T>([NotNullAttribute] this IQueryable<T> source, [NotNullAttribute][NotParameterized] string tag);
        //
        // Summary:
        //     Creates a System.Collections.Generic.Dictionary`2 from an System.Linq.IQueryable`1
        //     by enumerating it asynchronously according to a specified key selector function.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create a System.Collections.Generic.Dictionary`2
        //     from.
        //
        //   keySelector:
        //     A function to extract a key from each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains a
        //     System.Collections.Generic.Dictionary`2 that contains selected keys and values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or keySelector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Creates a System.Collections.Generic.Dictionary`2 from an System.Linq.IQueryable`1
        //     by enumerating it asynchronously according to a specified key selector function
        //     and a comparer.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create a System.Collections.Generic.Dictionary`2
        //     from.
        //
        //   keySelector:
        //     A function to extract a key from each element.
        //
        //   comparer:
        //     An System.Collections.Generic.IEqualityComparer`1 to compare keys.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector .
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains a
        //     System.Collections.Generic.Dictionary`2 that contains selected keys and values.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or keySelector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Func<TSource, TKey> keySelector, [NotNullAttribute] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Creates a System.Collections.Generic.Dictionary`2 from an System.Linq.IQueryable`1
        //     by enumerating it asynchronously according to a specified key selector and an
        //     element selector function.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create a System.Collections.Generic.Dictionary`2
        //     from.
        //
        //   keySelector:
        //     A function to extract a key from each element.
        //
        //   elementSelector:
        //     A transform function to produce a result element value from each element.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector .
        //
        //   TElement:
        //     The type of the value returned by elementSelector.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains a
        //     System.Collections.Generic.Dictionary`2 that contains values of type TElement
        //     selected from the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or keySelector or elementSelector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Func<TSource, TKey> keySelector, [NotNullAttribute] Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Creates a System.Collections.Generic.Dictionary`2 from an System.Linq.IQueryable`1
        //     by enumerating it asynchronously according to a specified key selector function,
        //     a comparer, and an element selector function.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create a System.Collections.Generic.Dictionary`2
        //     from.
        //
        //   keySelector:
        //     A function to extract a key from each element.
        //
        //   elementSelector:
        //     A transform function to produce a result element value from each element.
        //
        //   comparer:
        //     An System.Collections.Generic.IEqualityComparer`1 to compare keys.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector .
        //
        //   TElement:
        //     The type of the value returned by elementSelector.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains a
        //     System.Collections.Generic.Dictionary`2 that contains values of type TElement
        //     selected from the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source or keySelector or elementSelector is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        [AsyncStateMachine(typeof(EntityFrameworkQueryableExtensions.< ToDictionaryAsync > d__96 <,,>))]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>([NotNullAttribute] this IQueryable<TSource> source, [NotNullAttribute] Func<TSource, TKey> keySelector, [NotNullAttribute] Func<TSource, TElement> elementSelector, [NotNullAttribute] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Asynchronously creates a System.Collections.Generic.List`1 from an System.Linq.IQueryable`1
        //     by enumerating it asynchronously.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable`1 to create a list from.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation. The task result contains a
        //     System.Collections.Generic.List`1 that contains elements from the input sequence.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        [AsyncStateMachine(typeof(EntityFrameworkQueryableExtensions.< ToListAsync > d__65<>))]
        public static Task<List<TSource>> ToListAsync<TSource>([NotNullAttribute] this IQueryable<TSource> source, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Generates a string representation of the query used. This string may not be suitable
        //     for direct execution is intended only for use in debugging.
        //     This is only typically supported by queries generated by Entity Framework Core.
        //
        // Parameters:
        //   source:
        //     The query source.
        //
        // Returns:
        //     The query string for debugging.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static string ToQueryString([NotNullAttribute] this IQueryable source);
        */
        #endregion
    }
}

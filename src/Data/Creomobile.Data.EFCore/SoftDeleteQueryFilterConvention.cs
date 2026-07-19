using System.Linq.Expressions;
using System.Reflection;
using Creomobile.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Creomobile.Data.EFCore;

/// <summary>
/// Hides soft-deleted entities from queries: every root entity type implementing
/// <see cref="IDeletedAt" /> gets a named query filter (<see cref="FilterKey" />)
/// of the form <c>e =&gt; e.DeletedAt == null</c>. Bypass it per query with
/// <c>IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])</c>.
/// </summary>
/// <remarks>
/// EF Core does not allow mixing anonymous and named query filters on one entity
/// type, so an <see cref="IDeletedAt" /> entity configuring its own filter must use
/// the named <c>HasQueryFilter(key, filter)</c> form — an anonymous filter fails
/// model building. Named filters configured by the application coexist with this
/// one, and a filter configured explicitly under <see cref="FilterKey" /> itself
/// replaces the convention's predicate — use that to customize soft-delete
/// semantics for an entity type. <see cref="IDeletedAt" /> on an owned type is
/// ignored: an owned entity shares its owner's lifecycle. A hierarchy may only
/// implement <see cref="IDeletedAt" /> starting at its root; other models fail
/// model building.
/// </remarks>
public sealed class SoftDeleteQueryFilterConvention : IModelFinalizingConvention
{
    /// <summary>
    /// Key of the query filter added by this convention.
    /// </summary>
    public const string FilterKey = "SoftDelete";

    private static readonly MethodInfo EfProperty =
        typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(DateTime?));

    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            // Query filters are not supported on owned types; an owned fragment
            // follows its owner's lifecycle anyway.
            if (entityType.IsOwned() || !typeof(IDeletedAt).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.BaseType is not null)
            {
                // The root's filter covers the whole hierarchy. A hierarchy that
                // starts soft deletion below its root cannot be filtered at all,
                // which would silently expose soft-deleted rows — reject loudly.
                if (!typeof(IDeletedAt).IsAssignableFrom(entityType.GetRootType().ClrType))
                    throw new InvalidOperationException(
                        $"Entity type '{entityType.DisplayName()}' implements '{nameof(IDeletedAt)}', but the root "
                        + $"of its hierarchy '{entityType.GetRootType().DisplayName()}' does not. Query filters "
                        + $"apply to hierarchy roots only — implement '{nameof(IDeletedAt)}' on the root type.");
                continue;
            }

            var property = entityType.FindProperty(nameof(IDeletedAt.DeletedAt))
                           ?? throw new InvalidOperationException(
                               $"Entity type '{entityType.DisplayName()}' implements '{nameof(IDeletedAt)}', but "
                               + $"has no mapped '{nameof(IDeletedAt.DeletedAt)}' property.");

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // Bind to the mapped property: the CLR member when one backs it (this
            // covers explicit interface implementations), EF.Property for shadow
            // properties.
            var access = property.PropertyInfo is { } clrProperty
                ? Expression.Property(parameter, clrProperty)
                : (Expression)Expression.Call(EfProperty, parameter, Expression.Constant(property.Name));

            var filter = Expression.Lambda(
                Expression.Equal(access, Expression.Constant(null, typeof(DateTime?))),
                parameter);

            entityType.Builder.HasQueryFilter(FilterKey, filter);
        }
    }
}

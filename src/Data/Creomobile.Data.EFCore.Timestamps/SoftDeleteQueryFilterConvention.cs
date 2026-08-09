using System.Linq.Expressions;
using System.Reflection;
using Creomobile.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Creomobile.Data.EFCore.Timestamps;

/// <summary>
/// Hides soft-deleted entities from queries: every non-owned hierarchy root
/// implementing <see cref="IDeletedAt" /> gets a named query filter
/// (<see cref="FilterKey" />) of the form <c>e =&gt; e.DeletedAt == null</c>.
/// </summary>
/// <remarks>
/// Bypass the filter per query with <c>IgnoreQueryFilters()</c> or selectively
/// with <c>IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])</c>.
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
    /// Name of the query filter added by this convention (<c>"SoftDelete"</c>).
    /// Pass it to <c>IgnoreQueryFilters</c> to bypass the filter selectively.
    /// </summary>
    public const string FilterKey = "SoftDelete";

    static readonly MethodInfo EfProperty =
        typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(DateTime?));

    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        // Query filters are not supported on owned types; an owned fragment
        // follows its owner's lifecycle anyway.
        var deletables = modelBuilder.Metadata.GetEntityTypes()
            .Where(t => !t.IsOwned() && typeof(IDeletedAt).IsAssignableFrom(t.ClrType))
            .ToList();

        // The root's filter covers the whole hierarchy. A hierarchy that starts
        // soft deletion below its root cannot be filtered at all, which would
        // silently expose soft-deleted rows — reject loudly.
        var orphan = deletables.FirstOrDefault(t =>
            t.BaseType is not null && !typeof(IDeletedAt).IsAssignableFrom(t.GetRootType().ClrType));
        if (orphan is not null)
            throw new InvalidOperationException(
                $"Entity type '{orphan.DisplayName()}' implements '{nameof(IDeletedAt)}', but the root "
                + $"of its hierarchy '{orphan.GetRootType().DisplayName()}' does not. Query filters "
                + $"apply to hierarchy roots only — implement '{nameof(IDeletedAt)}' on the root type.");

        var filters = (
            from entityType in deletables
            where entityType.BaseType is null
            let property = entityType.FindProperty(nameof(IDeletedAt.DeletedAt))
                           ?? throw new InvalidOperationException(
                               $"Entity type '{entityType.DisplayName()}' implements '{nameof(IDeletedAt)}', but "
                               + $"has no mapped '{nameof(IDeletedAt.DeletedAt)}' property.")
            select (EntityType: entityType, Filter: BuildFilter(entityType, property))).ToList();

        foreach (var (entityType, filter) in filters)
            entityType.Builder.HasQueryFilter(FilterKey, filter);
    }

    // Bind to the mapped property: the CLR member when one backs it (this
    // covers explicit interface implementations), EF.Property for shadow
    // properties.
    static LambdaExpression BuildFilter(IConventionEntityType entityType, IConventionProperty property)
    {
        var parameter = Expression.Parameter(entityType.ClrType, "e");

        Expression access = property.PropertyInfo is { } clrProperty
            ? Expression.Property(parameter, clrProperty)
            : Expression.Call(EfProperty, parameter, Expression.Constant(property.Name));

        return Expression.Lambda(
            Expression.Equal(access, Expression.Constant(null, typeof(DateTime?))),
            parameter);
    }
}

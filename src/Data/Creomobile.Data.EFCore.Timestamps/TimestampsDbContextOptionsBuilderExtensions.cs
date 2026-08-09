using Creomobile.Data.Abstractions;
using Creomobile.Data.EFCore.Timestamps;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Timestamp and soft-delete extensions for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class TimestampsDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Automatically maintains <see cref="ICreatedAt" />, <see cref="IUpdatedAt" /> and
    /// <see cref="IDeletedAt" /> timestamps of tracked entities when saving changes.
    /// </summary>
    /// <remarks>
    /// On insert, <see cref="ICreatedAt.CreatedAt" /> and <see cref="IUpdatedAt.UpdatedAt" />
    /// receive the same UTC instant. On update, <see cref="IUpdatedAt.UpdatedAt" /> is
    /// refreshed and <see cref="ICreatedAt.CreatedAt" /> is protected from overwrites.
    /// Deleting an <see cref="IDeletedAt" /> entity turns into an update that sets
    /// <see cref="IDeletedAt.DeletedAt" /> (soft delete); its owned entities are
    /// retained, and setting the timestamp back to <c>null</c> restores the entity.
    /// Regular (non-owned) dependents still follow their configured delete behavior —
    /// implement <see cref="IDeletedAt" /> on them or configure an appropriate
    /// <c>DeleteBehavior</c> if they must survive.
    /// Soft-deleted entities are hidden from queries by a model-level filter — see
    /// <see cref="SoftDeleteQueryFilterConvention" />. All entities saved together
    /// share one instant. Repeated calls are no-ops: the first call wins, including
    /// its <paramref name="timeProvider" />.
    /// </remarks>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    /// <param name="timeProvider">
    /// Source of time; defaults to <see cref="TimeProvider.System" />.
    /// </param>
    /// <returns>The same builder instance so that calls can be chained.</returns>
    public static DbContextOptionsBuilder UseTimestamps(
        this DbContextOptionsBuilder optionsBuilder,
        TimeProvider? timeProvider = null)
    {
        // Repeated calls must not stack a second interceptor.
        if (optionsBuilder.Options.FindExtension<TimestampsOptionsExtension>() is not null)
            return optionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new TimestampsOptionsExtension());
        return optionsBuilder.AddInterceptors(
            new TimestampsInterceptor(timeProvider ?? TimeProvider.System));
    }

    /// <inheritdoc cref="UseTimestamps(DbContextOptionsBuilder, TimeProvider?)" />
    /// <typeparam name="TContext">The <see cref="DbContext" /> type being configured.</typeparam>
    public static DbContextOptionsBuilder<TContext> UseTimestamps<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        TimeProvider? timeProvider = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseTimestamps((DbContextOptionsBuilder)optionsBuilder, timeProvider);
}

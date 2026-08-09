using Creomobile.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Creomobile.Data.EFCore.Timestamps;

/// <summary>
/// Maintains <see cref="ICreatedAt" />, <see cref="IUpdatedAt" /> and
/// <see cref="IDeletedAt" /> timestamps when saving changes. See
/// <c>UseTimestamps</c> for the full semantics.
/// </summary>
sealed class TimestampsInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    void ApplyTimestamps(DbContext? context)
    {
        if (context is null)
            return;

        // One instant per save: every entity written together gets the same value.
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // ToList: converting Deleted entries to Modified re-buckets them inside
        // the state manager, which would invalidate the live enumeration.
        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ICreatedAt)
                        entry.Property(nameof(ICreatedAt.CreatedAt)).CurrentValue = now;
                    if (entry.Entity is IUpdatedAt)
                        entry.Property(nameof(IUpdatedAt.UpdatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    StampUpdate(entry, now);
                    break;

                // Owned entries are excluded: an owned fragment shares its owner's
                // lifecycle (soft delete applies to the aggregate root), and
                // converting one independently would race the owner's resurrection
                // below — the outcome would depend on entry enumeration order.
                case EntityState.Deleted when entry.Entity is IDeletedAt && !entry.Metadata.IsOwned():
                    // Through Unchanged, not straight to Modified: that would mark
                    // every property modified and make the UPDATE write the whole
                    // row, silently overwriting concurrent database changes. From
                    // Unchanged, each CurrentValue write below marks just its own
                    // property and promotes the entry itself.
                    entry.State = EntityState.Unchanged;
                    entry.Property(nameof(IDeletedAt.DeletedAt)).CurrentValue = now;
                    if (entry.Entity is IUpdatedAt)
                        entry.Property(nameof(IUpdatedAt.UpdatedAt)).CurrentValue = now;
                    ResurrectOwnedEntries(entry);
                    break;
            }
        }
    }

    // Values are written through the tracked entry, not the CLR setter: DetectChanges
    // has already run by the time the interceptor is called, so a plain setter write
    // would not be picked up for the current save.
    static void StampUpdate(EntityEntry entry, DateTime now)
    {
        if (entry.Entity is IUpdatedAt)
            entry.Property(nameof(IUpdatedAt.UpdatedAt)).CurrentValue = now;

        if (entry.Entity is not ICreatedAt) return;

        // Restore the value, not just suppress the write: otherwise the
        // tracked entity would keep the rejected value in memory and
        // disagree with the database until reloaded.
        var createdAt = entry.Property(nameof(ICreatedAt.CreatedAt));
        createdAt.CurrentValue = createdAt.OriginalValue;
        createdAt.IsModified = false;
    }

    // Owned entities live inside the soft-deleted aggregate and must survive with
    // it: left Deleted, a table-split owned entry would null out its columns in
    // the owner's row, and a separately-tabled one would lose its rows.
    static void ResurrectOwnedEntries(EntityEntry entry)
    {
        foreach (var navigationEntry in entry.Navigations)
        {
            if (navigationEntry.Metadata is not INavigation { ForeignKey.IsOwnership: true, IsOnDependent: false })
                continue;

            var targets = navigationEntry switch
            {
                CollectionEntry { CurrentValue: { } items } => items.Cast<object>(),
                ReferenceEntry { CurrentValue: { } item } => [item],
                _ => []
            };

            foreach (var target in targets)
            {
                var targetEntry = entry.Context.Entry(target);
                if (targetEntry.State != EntityState.Deleted)
                    continue;

                targetEntry.State = EntityState.Unchanged;
                ResurrectOwnedEntries(targetEntry);
            }
        }
    }
}

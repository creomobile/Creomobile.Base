using Creomobile.Data.Abstractions;
using Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests;

public sealed class TimestampsTestContext(DbContextOptions<TimestampsTestContext> options)
    : DbContext(options)
{
    public DbSet<TimestampedEntity> TimestampedEntities => Set<TimestampedEntity>();

    public DbSet<StampsLookalike> StampsLookalikes => Set<StampsLookalike>();

    public DbSet<SoftDeleteOwner> SoftDeleteOwners => Set<SoftDeleteOwner>();

    public DbSet<CreatedOnlyEntity> CreatedOnlyEntities => Set<CreatedOnlyEntity>();

    public DbSet<UpdatedOnlyEntity> UpdatedOnlyEntities => Set<UpdatedOnlyEntity>();

    public DbSet<DoubleFilteredEntity> DoubleFilteredEntities => Set<DoubleFilteredEntity>();

    public DbSet<DeletableRoot> DeletableRoots => Set<DeletableRoot>();

    public DbSet<CascadeParent> CascadeParents => Set<CascadeParent>();

    public DbSet<CascadeChild> CascadeChildren => Set<CascadeChild>();

    public DbSet<ShadowDeletedAtEntity> ShadowDeletedAtEntities => Set<ShadowDeletedAtEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The CLR member is an explicit interface implementation, so this maps a shadow
        // property rather than binding to it.
        modelBuilder.Entity<ShadowDeletedAtEntity>()
            .Property<DateTime?>(nameof(IDeletedAt.DeletedAt));

        modelBuilder.Entity<SoftDeleteOwner>(builder =>
        {
            builder.OwnsOne(o => o.Details);
            builder.OwnsMany(o => o.Tags);
        });

        modelBuilder.Entity<DoubleFilteredEntity>()
            .HasQueryFilter("Category", e => e.Category != "hidden");

        modelBuilder.Entity<DeletableLeaf>();
    }
}

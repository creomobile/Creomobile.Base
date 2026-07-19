using Creomobile.Data.EFCore.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.IntegrationTests;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

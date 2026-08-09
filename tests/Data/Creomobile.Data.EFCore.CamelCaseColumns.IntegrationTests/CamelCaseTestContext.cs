using Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests;

public sealed class CamelCaseTestContext(DbContextOptions<CamelCaseTestContext> options)
    : DbContext(options)
{
    public DbSet<CamelCaseEntity> CamelCaseEntities => Set<CamelCaseEntity>();

    public DbSet<FluentNamedEntity> FluentNamedEntities => Set<FluentNamedEntity>();

    public DbSet<TphAnimal> TphAnimals => Set<TphAnimal>();

    public DbSet<CamelCaseOwner> CamelCaseOwners => Set<CamelCaseOwner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FluentNamedEntity>()
            .Property(e => e.FluentNamed)
            .HasColumnName("FluentLegacy");

        modelBuilder.Entity<TphCat>();
        modelBuilder.Entity<TphDog>();

        modelBuilder.Entity<CamelCaseOwner>().OwnsOne(o => o.Details);
    }
}

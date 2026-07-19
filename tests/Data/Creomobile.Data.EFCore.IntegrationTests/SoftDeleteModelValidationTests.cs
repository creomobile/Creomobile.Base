using Creomobile.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class SoftDeleteModelValidationTests(PostgresFixture postgresFixture)
{
    [Fact]
    public void ImplementingIDeletedAtBelowHierarchyRootFailsModelBuilding()
    {
        using var context = new InvalidHierarchyContext(CreateOptions<InvalidHierarchyContext>());

        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*implement 'IDeletedAt' on the root type*");
    }

    [Fact]
    public void UnmappedDeletedAtFailsModelBuilding()
    {
        using var context = new UnmappedDeletedAtContext(CreateOptions<UnmappedDeletedAtContext>());

        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*has no mapped 'DeletedAt' property*");
    }

    // Model building never opens a connection — the database stays untouched.
    private DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
        => new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(TestDatabase.ConnectionString(postgresFixture, "model_validation_tests"))
            .UseTimestamps()
            .Options;
}

// Invalid model fixtures live next to the tests that need them — they exist
// only to fail model building and must not look reusable.

public abstract class NonDeletableRoot
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = null!;
}

public sealed class DeletableBelowRoot : NonDeletableRoot, IDeletedAt
{
    public DateTime? DeletedAt { get; set; }
}

public sealed class ExplicitlyImplementedDeletedAt : IDeletedAt
{
    public int Id { get; set; }

    DateTime? IDeletedAt.DeletedAt { get; set; }
}

public sealed class InvalidHierarchyContext(DbContextOptions<InvalidHierarchyContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NonDeletableRoot>();
        modelBuilder.Entity<DeletableBelowRoot>();
    }
}

public sealed class UnmappedDeletedAtContext(DbContextOptions<UnmappedDeletedAtContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ExplicitlyImplementedDeletedAt>();
}

using Creomobile.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests;

// Model building only — no connection is ever opened, so this class takes no fixture and
// needs no container. It used to ask the assembly fixture for a connection string it never
// used, which cost nothing in time (the container is shared and starts anyway) and cost a
// reader the truth: the class looked like a database test and is not one.
public sealed class SoftDeleteModelValidationTests
{
    // Never dialled. Npgsql only has to accept the SHAPE of it, because building a model
    // needs a provider, not a server.
    const string UnusedConnectionString =
        "Host=model-validation.invalid;Database=unused;Username=unused;Password=unused";

    [Fact]
    public void ImplementingIDeletedAtBelowHierarchyRootFailsModelBuilding()
    {
        using var context = new InvalidHierarchyContext(CreateOptions<InvalidHierarchyContext>());

        // ReSharper disable once AccessToDisposedClosure
        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*implement 'IDeletedAt' on the root type*");
    }

    [Fact]
    public void UnmappedDeletedAtFailsModelBuilding()
    {
        using var context = new UnmappedDeletedAtContext(CreateOptions<UnmappedDeletedAtContext>());

        // ReSharper disable once AccessToDisposedClosure
        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*has no mapped 'DeletedAt' property*");
    }

    static DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
        => new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(UnusedConnectionString)
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

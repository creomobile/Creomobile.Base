using Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests;

public sealed class SoftDeleteQueryFilterTests(PostgresFixture postgresFixture)
{
    const string Database = "soft_delete_tests";

    [Fact]
    public async Task SoftDeletedEntitiesAreHiddenFromQueries()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAndSoftDeleteAsync(cancellationToken);

        await using var context = await CreateContextAsync(cancellationToken);
        (await context.TimestampedEntities.AnyAsync(e => e.Id == id, cancellationToken))
            .Should().BeFalse();
    }

    [Fact]
    public async Task IgnoreQueryFiltersRevealsSoftDeletedEntities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAndSoftDeleteAsync(cancellationToken);

        await using var context = await CreateContextAsync(cancellationToken);

        (await context.TimestampedEntities
                .IgnoreQueryFilters()
                .AnyAsync(e => e.Id == id, cancellationToken))
            .Should().BeTrue();

        (await context.TimestampedEntities
                .IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])
                .AnyAsync(e => e.Id == id, cancellationToken))
            .Should().BeTrue();
    }

    [Fact]
    public async Task IgnoringSoftDeleteFilterKeepsOtherNamedFilters()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        int visibleId;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var visible = new DoubleFilteredEntity { Category = "visible" };
            var hidden = new DoubleFilteredEntity { Category = "hidden" };
            context.AddRange(visible, hidden);
            await context.SaveChangesAsync(cancellationToken);
            visibleId = visible.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var visible = await context.DoubleFilteredEntities
                .SingleAsync(e => e.Id == visibleId, cancellationToken);
            context.Remove(visible);
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            // Both filters active: one row is soft-deleted, the other category-filtered.
            (await context.DoubleFilteredEntities.AnyAsync(cancellationToken))
                .Should().BeFalse();

            // Lifting only the soft-delete filter keeps the category filter working.
            var revealed = await context.DoubleFilteredEntities
                .IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])
                .ToListAsync(cancellationToken);
            revealed.Should().ContainSingle();
            revealed[0].Id.Should().Be(visibleId);
            revealed[0].DeletedAt.Should().NotBeNull();

            // Lifting everything reveals both rows.
            (await context.DoubleFilteredEntities.IgnoreQueryFilters().CountAsync(cancellationToken))
                .Should().Be(2);
        }
    }

    [Fact]
    public async Task HierarchyRootFilterCoversDerivedEntities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        int id;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var leaf = new DeletableLeaf { Payload = "leaf", Extra = "x" };
            context.Add(leaf);
            await context.SaveChangesAsync(cancellationToken);
            id = leaf.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var leaf = await context.DeletableRoots.SingleAsync(e => e.Id == id, cancellationToken);
            leaf.CreatedAt.Should().NotBe(default);
            context.Remove(leaf);
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            (await context.DeletableRoots.AnyAsync(e => e.Id == id, cancellationToken))
                .Should().BeFalse();

            var revealed = await context.DeletableRoots
                .IgnoreQueryFilters()
                .SingleAsync(e => e.Id == id, cancellationToken);
            revealed.DeletedAt.Should().NotBeNull();
            revealed.Should().BeOfType<DeletableLeaf>();
        }
    }

    async Task<TimestampsTestContext> CreateContextAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<TimestampsTestContext>()
            .UseNpgsql(TestDatabase.ConnectionString(postgresFixture, Database))
            .UseTimestamps()
            .Options;

        var context = new TimestampsTestContext(options);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        return context;
    }

    async Task<int> InsertAndSoftDeleteAsync(CancellationToken cancellationToken)
    {
        int id;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = new TimestampedEntity { Payload = "filtered" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.TimestampedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            context.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }

        return id;
    }
}

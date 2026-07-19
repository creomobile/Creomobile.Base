using Creomobile.Data.EFCore.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class TimestampsInterceptorTests(PostgresFixture postgresFixture)
{
    private const string Database = "timestamps_tests";

    [Fact]
    public async Task InsertStampsCreatedAtAndUpdatedAtWithSameUtcInstant()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAsync("insert", cancellationToken);

        var entity = await LoadAsync(id, cancellationToken);

        entity.CreatedAt.Should().NotBe(default);
        entity.UpdatedAt.Should().Be(entity.CreatedAt);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRefreshesUpdatedAtAndKeepsCreatedAt()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var insertInstant = new DateTime(2030, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var updateInstant = insertInstant.AddHours(1);
        var clock = new MutableTimeProvider(insertInstant);

        int id;
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = new TimestampedEntity { Payload = "before" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        clock.UtcNow = updateInstant;
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = await context.TimestampedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.Payload = "after";
            await context.SaveChangesAsync(cancellationToken);
        }

        var updated = await LoadAsync(id, cancellationToken);
        updated.CreatedAt.Should().Be(insertInstant);
        updated.UpdatedAt.Should().Be(updateInstant);
        updated.Payload.Should().Be("after");
    }

    [Fact]
    public async Task EntitiesSavedTogetherShareOneInstant()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var start = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        int firstId, secondId;
        await using (var context = await CreateContextAsync(cancellationToken, new TickingTimeProvider(start)))
        {
            var first = new TimestampedEntity { Payload = "first" };
            var second = new TimestampedEntity { Payload = "second" };
            context.AddRange(first, second);
            await context.SaveChangesAsync(cancellationToken);
            firstId = first.Id;
            secondId = second.Id;
        }

        // The ticking clock returns a later instant on every call: had the
        // interceptor asked once per entity, the stamps would differ.
        (await LoadAsync(firstId, cancellationToken)).CreatedAt.Should().Be(start);
        (await LoadAsync(secondId, cancellationToken)).CreatedAt.Should().Be(start);
    }

    [Fact]
    public async Task CreatedAtOnlyInterfaceIsStampedIndependently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var insertInstant = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var clock = new MutableTimeProvider(insertInstant);

        int id;
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = new CreatedOnlyEntity { Payload = "created-only" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        clock.UtcNow = insertInstant.AddHours(1);
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = await context.CreatedOnlyEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.Payload = "changed";
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.CreatedOnlyEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.CreatedAt.Should().Be(insertInstant);
            entity.Payload.Should().Be("changed");
        }
    }

    [Fact]
    public async Task UpdatedAtOnlyInterfaceIsStampedIndependently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var insertInstant = new DateTime(2030, 7, 1, 8, 0, 0, DateTimeKind.Utc);
        var updateInstant = insertInstant.AddHours(1);
        var clock = new MutableTimeProvider(insertInstant);

        int id;
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = new UpdatedOnlyEntity { Payload = "updated-only" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.UpdatedOnlyEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.UpdatedAt.Should().Be(insertInstant);
        }

        clock.UtcNow = updateInstant;
        await using (var context = await CreateContextAsync(cancellationToken, clock))
        {
            var entity = await context.UpdatedOnlyEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.Payload = "changed";
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.UpdatedOnlyEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.UpdatedAt.Should().Be(updateInstant);
        }
    }

    [Fact]
    public async Task UpdateCannotOverwriteCreatedAt()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAsync("guarded", cancellationToken);
        var original = await LoadAsync(id, cancellationToken);

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.TimestampedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            entity.CreatedAt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            entity.Payload = "tampered";
            await context.SaveChangesAsync(cancellationToken);
        }

        var updated = await LoadAsync(id, cancellationToken);
        updated.CreatedAt.Should().Be(original.CreatedAt);
        updated.Payload.Should().Be("tampered");
    }

    [Fact]
    public async Task DeleteBecomesSoftDeleteKeepingTheRow()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAsync("victim", cancellationToken);

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.TimestampedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            context.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }

        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);
        var rows = await TestDatabase.CountRowsAsync(
            connectionString, "TimestampedEntities", "Id", id, cancellationToken);
        rows.Should().Be(1);

        var deleted = await LoadAsync(id, cancellationToken, ignoreQueryFilters: true);
        deleted.DeletedAt.Should().NotBeNull();
        deleted.UpdatedAt.Should().Be(deleted.DeletedAt!.Value);
    }

    [Fact]
    public async Task ClearingDeletedAtRestoresTheEntity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = await InsertAsync("phoenix", cancellationToken);

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.TimestampedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            context.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var entity = await context.TimestampedEntities
                .IgnoreQueryFilters()
                .SingleAsync(e => e.Id == id, cancellationToken);
            entity.DeletedAt = null;
            await context.SaveChangesAsync(cancellationToken);
        }

        var restored = await LoadAsync(id, cancellationToken);
        restored.Payload.Should().Be("phoenix");
        restored.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteKeepsOwnedData()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        int id;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var owner = new SoftDeleteOwner
            {
                Title = "aggregate",
                Details = new OwnedDetails { City = "Lisbon" },
                Tags = [new OwnedTag { Label = "keep" }, new OwnedTag { Label = "me" }],
            };
            context.Add(owner);
            await context.SaveChangesAsync(cancellationToken);
            id = owner.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var owner = await context.SoftDeleteOwners.SingleAsync(o => o.Id == id, cancellationToken);
            context.Remove(owner);
            await context.SaveChangesAsync(cancellationToken);
        }

        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);
        var rows = await TestDatabase.CountRowsAsync(
            connectionString, "SoftDeleteOwners", "Id", id, cancellationToken);
        rows.Should().Be(1);

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var owner = await context.SoftDeleteOwners
                .IgnoreQueryFilters()
                .SingleAsync(o => o.Id == id, cancellationToken);
            owner.DeletedAt.Should().NotBeNull();
            owner.Details.City.Should().Be("Lisbon");

            // The separately-tabled owned collection must survive as well.
            owner.Tags.Select(t => t.Label).Should().BeEquivalentTo("keep", "me");
        }
    }

    [Fact]
    public async Task LookalikeWithoutInterfacesIsNeverTouched()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var preset = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        int id;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var lookalike = new StampsLookalike { CreatedAt = preset, UpdatedAt = preset };
            context.Add(lookalike);
            await context.SaveChangesAsync(cancellationToken);
            id = lookalike.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var lookalike = await context.StampsLookalikes.SingleAsync(e => e.Id == id, cancellationToken);
            lookalike.CreatedAt.Should().Be(preset);
            lookalike.UpdatedAt.Should().Be(preset);

            lookalike.DeletedAt = preset;
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            // No query filter: the manually marked row stays visible.
            var lookalike = await context.StampsLookalikes.SingleAsync(e => e.Id == id, cancellationToken);
            lookalike.DeletedAt.Should().Be(preset);

            context.Remove(lookalike);
            await context.SaveChangesAsync(cancellationToken);
        }

        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);
        var rows = await TestDatabase.CountRowsAsync(
            connectionString, "StampsLookalikes", "Id", id, cancellationToken);
        rows.Should().Be(0);
    }

    [Fact]
    public async Task TimestampsComeFromTheConfiguredTimeProvider()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var fixedInstant = new DateTime(2031, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        int id;
        await using (var context = await CreateContextAsync(cancellationToken, new MutableTimeProvider(fixedInstant)))
        {
            var entity = new TimestampedEntity { Payload = "frozen clock" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        var entity2 = await LoadAsync(id, cancellationToken);
        entity2.CreatedAt.Should().Be(fixedInstant);
        entity2.UpdatedAt.Should().Be(fixedInstant);
    }

    [Fact]
    public async Task SynchronousSaveChangesStampsToo()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var instant = new DateTime(2030, 8, 1, 12, 0, 0, DateTimeKind.Utc);

        int id;
        await using (var context = await CreateContextAsync(cancellationToken, new MutableTimeProvider(instant)))
        {
            var entity = new TimestampedEntity { Payload = "sync" };
            context.Add(entity);
            context.SaveChanges();
            id = entity.Id;
        }

        var reloaded = await LoadAsync(id, cancellationToken);
        reloaded.CreatedAt.Should().Be(instant);
        reloaded.UpdatedAt.Should().Be(instant);
    }

    [Fact]
    public async Task RepeatedUseTimestampsKeepsTheFirstTimeProvider()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstInstant = new DateTime(2030, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var secondInstant = firstInstant.AddYears(1);

        var options = new DbContextOptionsBuilder<TimestampsTestContext>()
            .UseNpgsql(TestDatabase.ConnectionString(postgresFixture, Database))
            .UseTimestamps(new MutableTimeProvider(firstInstant))
            .UseTimestamps(new MutableTimeProvider(secondInstant))
            .Options;

        int id;
        await using (var context = new TimestampsTestContext(options))
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
            var entity = new TimestampedEntity { Payload = "first wins" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        var reloaded = await LoadAsync(id, cancellationToken);
        reloaded.CreatedAt.Should().Be(firstInstant);
    }

    [Fact]
    public async Task RegularDependentsFollowNormalCascadeOnSoftDelete()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        int parentId;
        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var parent = new CascadeParent
            {
                Label = "parent",
                Children = [new CascadeChild { Label = "child" }],
            };
            context.Add(parent);
            await context.SaveChangesAsync(cancellationToken);
            parentId = parent.Id;
        }

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            var parent = await context.CascadeParents
                .Include(p => p.Children)
                .SingleAsync(p => p.Id == parentId, cancellationToken);
            context.Remove(parent);
            await context.SaveChangesAsync(cancellationToken);
        }

        // The parent is retained (soft delete), a regular — not owned — child
        // follows the normal cascade and is physically gone.
        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);
        var parentRows = await TestDatabase.CountRowsAsync(
            connectionString, "CascadeParents", "Id", parentId, cancellationToken);
        parentRows.Should().Be(1);

        await using (var context = await CreateContextAsync(cancellationToken))
        {
            (await context.CascadeChildren.AnyAsync(cancellationToken)).Should().BeFalse();
        }
    }

    private async Task<TimestampsTestContext> CreateContextAsync(
        CancellationToken cancellationToken, TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<TimestampsTestContext>()
            .UseNpgsql(TestDatabase.ConnectionString(postgresFixture, Database))
            .UseTimestamps(timeProvider)
            .Options;

        var context = new TimestampsTestContext(options);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        return context;
    }

    private async Task<int> InsertAsync(string payload, CancellationToken cancellationToken)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        var entity = new TimestampedEntity { Payload = payload };
        context.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private async Task<TimestampedEntity> LoadAsync(
        int id, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        var query = ignoreQueryFilters
            ? context.TimestampedEntities.IgnoreQueryFilters()
            : context.TimestampedEntities;
        return await query.SingleAsync(e => e.Id == id, cancellationToken);
    }

    private sealed class MutableTimeProvider(DateTime utcNow) : TimeProvider
    {
        public DateTime UtcNow { get; set; } = utcNow;

        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }

    private sealed class TickingTimeProvider(DateTime startUtcNow) : TimeProvider
    {
        private DateTime _current = startUtcNow;

        public override DateTimeOffset GetUtcNow()
        {
            var value = _current;
            _current = value.AddMinutes(1);
            return new DateTimeOffset(value);
        }
    }
}

using Creomobile.Data.EFCore.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CombinedConventionsTests(PostgresFixture postgresFixture)
{
    private const string Database = "combined_tests";

    [Fact]
    public async Task CamelCaseAndTimestampsWorkTogether()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);
        var options = new DbContextOptionsBuilder<CombinedTestContext>()
            .UseNpgsql(connectionString)
            .UseCamelCaseColumnNames()
            .UseTimestamps()
            .Options;

        int id;
        await using (var context = new CombinedTestContext(options))
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
            var entity = new CombinedEntity { Payload = "both" };
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            id = entity.Id;
        }

        var columns = await TestDatabase.GetTableColumnsAsync(
            connectionString, "CombinedEntities", cancellationToken);
        columns.Should().BeEquivalentTo("id", "payload", "createdAt", "updatedAt", "deletedAt");

        await using (var context = new CombinedTestContext(options))
        {
            var entity = await context.CombinedEntities.SingleAsync(e => e.Id == id, cancellationToken);
            context.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }

        await using (var context = new CombinedTestContext(options))
        {
            (await context.CombinedEntities.AnyAsync(e => e.Id == id, cancellationToken))
                .Should().BeFalse();

            var deleted = await context.CombinedEntities
                .IgnoreQueryFilters()
                .SingleAsync(e => e.Id == id, cancellationToken);
            deleted.DeletedAt.Should().NotBeNull();
        }

        var rows = await TestDatabase.CountRowsAsync(
            connectionString, "CombinedEntities", "id", id, cancellationToken);
        rows.Should().Be(1);
    }
}

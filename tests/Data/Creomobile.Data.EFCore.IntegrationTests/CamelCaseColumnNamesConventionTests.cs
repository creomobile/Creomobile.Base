using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CamelCaseColumnNamesConventionTests(PostgresFixture postgresFixture)
{
    [Fact]
    public async Task CreatesCamelCaseColumnsForDefaultNamesOnly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = TestDatabase.ConnectionString(postgresFixture, "camel_case_tests");

        var options = new DbContextOptionsBuilder<CamelCaseTestContext>()
            .UseNpgsql(connectionString)
            .UseCamelCaseColumnNames()
            .Options;

        await using var context = new CamelCaseTestContext(options);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var columns = await TestDatabase.GetTableColumnsAsync(
            connectionString, "CamelCaseEntities", cancellationToken);

        columns.Should().BeEquivalentTo("id", "conventionNamed", "LegacyName");
    }
}

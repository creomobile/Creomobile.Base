using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CamelCaseColumnNamesConventionTests(PostgresFixture postgresFixture)
{
    [Fact]
    public async Task CreatesCamelCaseColumnsForDefaultNamesOnly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = new NpgsqlConnectionStringBuilder(
            postgresFixture.Container.GetConnectionString())
        {
            Database = "convention_tests",
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<ConventionTestContext>()
            .UseNpgsql(connectionString)
            .UseCamelCaseColumnNames()
            .Options;

        await using var context = new ConventionTestContext(options);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var columns = await GetTableColumnsAsync(connectionString, "Customers", cancellationToken);

        columns.Should().BeEquivalentTo("id", "name", "LegacyName");
    }

    static async Task<List<string>> GetTableColumnsAsync(
        string connectionString, string tableName, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            """
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = $1
            """,
            connection);
        command.Parameters.AddWithValue(tableName);

        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            columns.Add(reader.GetString(0));

        return columns;
    }
}

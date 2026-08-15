using Npgsql;

namespace Creomobile.Data.EFCore.TestSupport;

// Raw-SQL assertions specific to these suites. Addressing a database is not here: that is
// PostgresFixture.GetConnectionString, and it lives in the package precisely because it was
// the one piece two repositories had copied verbatim.
public static class TestDatabase
{
    public static async Task<List<string>> GetTableColumnsAsync(
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

    // Identifiers are interpolated from test literals — quoting is enough here.
    public static async Task<long> CountRowsAsync(
        string connectionString,
        string tableName,
        string idColumnName,
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"""SELECT count(*) FROM "{tableName}" WHERE "{idColumnName}" = $1""",
            connection);
        command.Parameters.AddWithValue(id);

        return (long)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}

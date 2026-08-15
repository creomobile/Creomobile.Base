using DotNet.Testcontainers.Containers;
using Npgsql;

namespace Creomobile.Testing.Postgres.IntegrationTests;

public sealed class PostgresFixtureTests(PostgresAssemblyFixture postgresFixture)
{
    const string Database = "fixture_tests";

    [Fact]
    public async Task StartsAServerReachableOnTheDefaultDatabase()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var database = await ScalarAsync<string>(
            postgresFixture.Container.GetConnectionString(),
            "SELECT current_database()",
            cancellationToken);

        database.Should().Be("tests");
    }

    // The image is the one thing the package refuses to choose, so prove the declared one is
    // what actually runs — otherwise the constructor argument could be quietly ignored and
    // every other test here would still pass. Asserting the major is enough for that: an
    // ignored argument would fall back to a different image entirely.
    [Fact]
    public async Task RunsTheMajorVersionTheAssemblyFixtureDeclares()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var major = await ScalarAsync<int>(
            postgresFixture.Container.GetConnectionString(),
            "SELECT current_setting('server_version_num')::int / 10000",
            cancellationToken);

        major.Should().Be(18);
    }

    [Fact]
    public void ContainerIsRunningOnceTheFixtureIsInitialized()
        => postgresFixture.Container.State.Should().Be(TestcontainersStates.Running);

    [Fact]
    public async Task GetConnectionStringTargetsTheNamedDatabaseAndKeepsTheRestOfTheString()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // The fixture names a database; creating it is the caller's job — the behaviour the
        // README promises, and the reason this test creates it first.
        await ExecuteAsync(
            postgresFixture.Container.GetConnectionString(),
            $"""CREATE DATABASE "{Database}" """,
            cancellationToken);

        var connectionString = postgresFixture.GetConnectionString(Database);

        var database = await ScalarAsync<string>(
            connectionString, "SELECT current_database()", cancellationToken);

        database.Should().Be(Database);

        // Host, port and credentials must survive the rewrite, or the connection above could
        // not have been opened — assert them anyway, because a silent fallback to another
        // server is exactly the failure this helper could hide.
        var rewritten = new NpgsqlConnectionStringBuilder(connectionString);
        var original = new NpgsqlConnectionStringBuilder(
            postgresFixture.Container.GetConnectionString());

        rewritten.Host.Should().Be(original.Host);
        rewritten.Port.Should().Be(original.Port);
        rewritten.Username.Should().Be(original.Username);
        rewritten.Password.Should().Be(original.Password);
    }

    // The reason the helper builds the string with DbConnectionStringBuilder instead of
    // concatenating: a name carrying a separator or a quote has to come back out intact,
    // and string surgery is where that quietly stops being true.
    [Theory]
    [InlineData("with;semicolon")]
    [InlineData("with'quote")]
    [InlineData("with\"double\"quote")]
    [InlineData("with space")]
    [InlineData("with=equals")]
    public void GetConnectionStringEscapesNamesThatWouldBreakTheString(string database)
        => new NpgsqlConnectionStringBuilder(postgresFixture.GetConnectionString(database))
            .Database.Should().Be(database);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetConnectionStringRejectsABlankName(string database)
        => postgresFixture.Invoking(fixture => fixture.GetConnectionString(database))
            .Should().Throw<ArgumentException>();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructionRejectsABlankImage(string image)
        => FluentActions.Invoking(() => new ImageProbeFixture(image))
            .Should().Throw<ArgumentException>();

    // The failure that actually happens — Docker cannot produce the image — must surface as
    // itself. The fixture disposes the half-started container on the way out, and that cleanup
    // must not become the exception the developer sees.
    [Fact]
    public async Task StartupFailureSurfacesInsteadOfBeingLostInCleanup()
    {
        var fixture = new ImageProbeFixture("creomobile-does-not-exist/postgres:no-such-tag");

        var thrown = await FluentActions.Awaiting(() => fixture.InitializeAsync().AsTask())
            .Should().ThrowAsync<Exception>();

        thrown.Which.Should().NotBeOfType<NullReferenceException>();
        thrown.Which.Should().NotBeOfType<ObjectDisposedException>();
    }

    // Only reachable because the base type is abstract by design: this is the smallest legal
    // consumer of it, standing in for a repository's own fixture.
    sealed class ImageProbeFixture(string image) : PostgresFixture(image);

    static async Task ExecuteAsync(
        string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    static async Task<T?> ScalarAsync<T>(
        string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is null or DBNull ? default : (T)result;
    }
}

using System.Data.Common;
using Testcontainers.PostgreSql;
using Xunit;

namespace Creomobile.Testing.Postgres;

/// <summary>
/// A PostgreSQL container shared by every test in one assembly.
/// </summary>
/// <remarks>
/// Derive from this type to declare which image the tests run against, then register the
/// derived type per test assembly with
/// <c>[assembly: AssemblyFixture(typeof(YourFixture))]</c> — an assembly-level attribute
/// applies only to the assembly it is compiled into, so it cannot live in this package. The
/// consequence is one container per test assembly, which test classes then share by taking
/// the derived type as a constructor parameter.
/// <code>
/// public sealed class PostgresAssemblyFixture() : PostgresFixture(
///     "postgres:18.4@sha256:&lt;the digest of the image your production runs&gt;");
/// </code>
/// <para>
/// This package has no default image and never picks one for you: which database version a
/// repository tests against is that repository's statement about the production it targets,
/// and a shared library has no business choosing it. The digest above is a placeholder for
/// yours — read it with <c>docker inspect --format='{{index .RepoDigests 0}}' postgres:18.4</c>
/// after pulling the image your production runs.
/// </para>
/// <para>
/// Pin by digest rather than by tag. A tag is a name and can be re-published against a
/// different image, so an exact patch tag still leaves "the same commit ran the same server"
/// resting on nobody having moved it; a digest is the image's content fingerprint and cannot
/// be moved. A major-only tag such as <c>postgres:18</c> is worse again — it is meant to move,
/// and Docker will not re-pull a tag it already has, so one machine can sit on a months-old
/// copy while another gets today's.
/// </para>
/// <para>
/// The fixture starts a database server; it does not create databases. Use
/// <see cref="GetConnectionString"/> to address a database of your own and create it yourself
/// (an ORM's "ensure created", a migration run, or plain SQL). Test classes that run
/// concurrently must use distinct database names.
/// </para>
/// <para>Requires a reachable Docker daemon; the image is pulled if it is not local.</para>
/// </remarks>
public abstract class PostgresFixture : IAsyncLifetime
{
    // Container-local throwaways, not a statement about production: the server is reachable
    // only on a random host port and lives for the length of one test assembly. Unlike the
    // image, nothing is gained by letting each repository restate them.
    const string Database = "tests";
    const string Username = "postgres";
    const string Password = "postgres";

    /// <summary>Builds the container. Starting it is <see cref="InitializeAsync"/>'s job.</summary>
    /// <param name="image">
    /// The full image reference to run — registry and repository included, so a mirror or a
    /// private registry can be named. Prefer a digest, as in
    /// <c>postgres:18.4@sha256:&lt;digest&gt;</c>: a tag can be re-published against a
    /// different image, a digest cannot.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="image"/> is null, empty or whitespace.</exception>
    protected PostgresFixture(string image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(image);

        Container = new PostgreSqlBuilder(image)
            .WithDatabase(Database)
            .WithUsername(Username)
            .WithPassword(Password)
            .WithPortBinding(5432, true)
            .WithLogger(new XunitDiagnosticLogger())
            .Build();
    }

    /// <summary>
    /// The container, for what this fixture does not wrap — running a script, reading logs.
    /// It exists from construction but is only running once <see cref="InitializeAsync"/> has
    /// completed, and it is disposed by the fixture, never by a test.
    /// </summary>
    public PostgreSqlContainer Container { get; }

    /// <summary>
    /// The container's connection string, pointed at <paramref name="database"/> instead of the
    /// default one. The database is not created by this call.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="database"/> is null, empty or whitespace — which would silently leave the
    /// connection string pointing at the default database.
    /// </exception>
    public string GetConnectionString(string database)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = Container.GetConnectionString(),
        };

        builder["Database"] = database;

        return builder.ConnectionString;
    }

    /// <summary>Starts the container.</summary>
    public async ValueTask InitializeAsync()
    {
        try
        {
            await Container.StartAsync(TestContext.Current.CancellationToken);
        }
        catch
        {
            // A container that failed to start still holds resources, and xunit does not call
            // DisposeAsync for a fixture whose initialization threw.
            try
            {
                await Container.DisposeAsync();
            }
            catch (Exception cleanupFailure)
            {
                // Never let a cleanup failure replace the startup one: that is the exception
                // saying why Docker refused, and it is the only one worth reading.
                TestContext.Current.SendDiagnosticMessage(
                    "[testcontainers] cleanup after a failed start also failed: {0}", cleanupFailure);
            }

            throw;
        }
    }

    /// <summary>Stops and removes the container.</summary>
    /// <remarks>
    /// Override <see cref="DisposeAsyncCore"/> rather than this method to release what a derived
    /// fixture owns: suppressing finalization here would otherwise silence a derived finalizer
    /// whose cleanup never ran.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases what this fixture owns. A derived fixture that holds resources of its own
    /// overrides this and calls the base implementation.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore() => Container.DisposeAsync();
}

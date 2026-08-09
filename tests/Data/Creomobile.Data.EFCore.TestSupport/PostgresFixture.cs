using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace Creomobile.Data.EFCore.TestSupport;

/// <summary>
/// Postgres container shared by every test in one assembly. Each test assembly registers it
/// itself with <c>[assembly: AssemblyFixture(typeof(PostgresFixture))]</c> — an assembly-level
/// attribute applies only to the assembly it is compiled into, so it cannot live here.
/// </summary>
public sealed class PostgresFixture(IMessageSink sink) : ContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(sink)
{
    const string Db = "tests";
    const string User = "postgres";
    const string Password = "postgres";

    protected override PostgreSqlBuilder Configure() => new PostgreSqlBuilder("postgres:18.4")
        .WithDatabase(Db)
        .WithUsername(User)
        .WithPassword(Password)
        .WithPortBinding(5432, true);
}

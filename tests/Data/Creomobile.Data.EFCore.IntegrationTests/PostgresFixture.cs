using Creomobile.Data.EFCore.IntegrationTests;
using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

[assembly: AssemblyFixture(typeof(PostgresFixture))]

namespace Creomobile.Data.EFCore.IntegrationTests;

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

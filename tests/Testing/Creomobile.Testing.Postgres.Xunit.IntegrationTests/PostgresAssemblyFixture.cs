namespace Creomobile.Testing.Postgres.IntegrationTests;

/// <summary>
/// The image this repository's tests run against: the exact patch production is running, not a
/// floating major — see the same declaration in
/// <c>Creomobile.Data.EFCore.TestSupport</c> for why.
/// </summary>
public sealed class PostgresAssemblyFixture() : PostgresFixture("postgres:18.4");

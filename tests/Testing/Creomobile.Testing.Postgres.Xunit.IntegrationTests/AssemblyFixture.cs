using Creomobile.Testing.Postgres.IntegrationTests;
using Xunit.Sdk;

// An assembly-level attribute applies only to the assembly it is compiled into, so each test
// assembly registers the shared fixture itself. Consequence: one Postgres container per assembly.
[assembly: AssemblyFixture(typeof(PostgresAssemblyFixture))]

using Creomobile.Testing.Postgres;

namespace Creomobile.Data.EFCore.TestSupport;

/// <summary>
/// The image this repository's tests run against: the exact patch production is running, not a
/// floating major.
/// </summary>
/// <remarks>
/// A major-only tag looks more honest — the major is what we declare to the managed provider,
/// and the patch is applied by it — but it moves under the tests. Verified on 2026-08-15:
/// <c>postgres:18</c> resolved to 18.4 on this machine from a July cache while the registry had
/// already moved that tag on, so the same commit would have run a different server in CI. The
/// exact tag is what makes a red test mean a code change. It follows production's current patch
/// and is refreshed deliberately, the same way the local development database is.
/// <para>
/// Each test assembly still registers this itself with
/// <c>[assembly: AssemblyFixture(typeof(PostgresAssemblyFixture))]</c> — an assembly-level
/// attribute applies only to the assembly it is compiled into. Consequence: one container per
/// test assembly. Not the generic <c>AssemblyFixture&lt;T&gt;</c> that xunit 4 adds: analyzer
/// xUnit1041 does not recognise it as a fixture source, and warnings are errors here.
/// </para>
/// </remarks>
public sealed class PostgresAssemblyFixture() : PostgresFixture("postgres:18.4");

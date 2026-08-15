using Creomobile.Testing.Postgres;

namespace Creomobile.Data.EFCore.TestSupport;

/// <summary>
/// The image this repository's tests run against: the image production runs, pinned by digest.
/// </summary>
/// <remarks>
/// A tag is a name and can be moved to another image; a digest is the image's content
/// fingerprint and cannot. That is the whole reason the digest is here — an exact patch tag
/// still leaves "the same commit ran the same server" resting on nobody having re-published
/// it. The tag is kept alongside so the line stays readable; only the digest binds. Refreshed
/// deliberately when production moves, the same way the local development database is.
/// <para>
/// Each test assembly still registers this itself with
/// <c>[assembly: AssemblyFixture(typeof(PostgresAssemblyFixture))]</c> — an assembly-level
/// attribute applies only to the assembly it is compiled into. Consequence: one container per
/// test assembly. Not the generic <c>AssemblyFixture&lt;T&gt;</c> that xunit 4 adds: analyzer
/// xUnit1041 does not recognise it as a fixture source, and warnings are errors here.
/// </para>
/// </remarks>
public sealed class PostgresAssemblyFixture() : PostgresFixture(
    "postgres:18.4@sha256:a02db8cac496f15b094798a38254f14d6e00741f709360e5e00bb6668ea31636");

# Creomobile.Testing.Postgres.Xunit

A PostgreSQL container for xunit integration tests, started once per test assembly via
[Testcontainers](https://dotnet.testcontainers.org/), plus a helper that points its
connection string at a database of your choosing.

## Usage

Derive from `PostgresFixture` to declare the image your tests run against, and register that
type once per test assembly — an assembly-level attribute applies only to the assembly it is
compiled into, so the package cannot do this for you:

```csharp
using Creomobile.Testing.Postgres;
using Xunit.Sdk;

[assembly: AssemblyFixture(typeof(PostgresAssemblyFixture))]

public sealed class PostgresAssemblyFixture() : PostgresFixture(
    "postgres:18.4@sha256:<the digest of the image your production runs>");
```

Then take **your** fixture type as a constructor parameter in any test class:

```csharp
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class CustomerTests(PostgresAssemblyFixture postgresFixture)
{
    const string Database = "customer_tests";

    [Fact]
    public async Task StoresACustomer()
    {
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(postgresFixture.GetConnectionString(Database))
                .Options);

        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // …
    }
}
```

## The version is yours, not this package's

`PostgresFixture` is abstract and takes the image as a required constructor argument. There
is **no default**: nothing in this package selects a PostgreSQL version, and no version is
reachable without your naming one. The tags in these examples are illustration, not a
fallback — delete the argument and the code does not compile.

That is deliberate. Which database version a repository tests against is a statement about
the production database that repository targets, and it belongs beside that repository's
code — a shared library choosing it for you is how tests and production drift apart without
anyone deciding to. The argument is a full image reference, so a mirror or a private registry
can be named too.

**Pin by digest, not by tag.** A tag is a name, and a name can be re-published against a
different image; a digest is the image's content fingerprint and cannot be moved. So even an
exact patch tag leaves "the same commit ran the same server" resting on nobody having moved
it. A floating major like `postgres:18` is worse — it is *meant* to move, and Docker will not
re-pull a tag it already has, so a developer's weeks-old cache and a fresh machine disagree
silently. A red test should mean a code change.

Read the digest of the image your production runs with:

```bash
docker pull postgres:18.4
docker inspect --format='{{index .RepoDigests 0}}' postgres:18.4
```

Keeping the tag in front of the digest costs nothing and keeps the line readable; only the
digest binds.

## What it does and does not do

- **Does:** start one PostgreSQL server for the whole test assembly, on a random host port,
  and stop it when the assembly finishes.
- **Does not:** create databases. `GetConnectionString(name)` only names one; creating it is
  yours to do — `EnsureCreatedAsync`, a migration run, or plain SQL.
- **Concurrency:** test classes in one assembly share the server, so classes that may run
  at the same time must use **distinct database names**.

The bootstrap database, username and password inside the container are fixed by the package
and not configurable: the server is reachable only on a random host port and lives for the
length of one test assembly, so they carry no decision worth restating per repository.

`Container` is exposed for everything this fixture does not wrap — running a script,
reading logs. It is disposed by the fixture; never dispose it from a test.

**Using it couples you to Testcontainers, and that is deliberate.** The property hands
back that library's own type rather than something of ours, so a major version of
Testcontainers is a breaking change for code that touches it — where code that stays on
`GetConnectionString` is unaffected. The alternative was a set of narrow wrappers for
needs nobody has demonstrated yet; an escape hatch you can see is better than one
guessed at in advance. If you find yourself needing something specific through it,
that is worth telling us: a named operation can then replace the raw handle.

## Requirements

- **.NET 10.** The package ships a `net10.0` assembly only — a test project on an earlier
  target framework cannot use it.
- A reachable Docker daemon. The image is pulled on first use if it is not already local.
- **xunit `3.2.x` or `4.x`.** One package serves both lines: it uses only `IAsyncLifetime`
  and `AssemblyFixture`, whose shapes are identical across them, and the type you register
  has a parameterless constructor, so xunit 4's `xUnit3005` rule is satisfied without any
  suppression. The dependency is on `xunit.v3.extensibility.core` alone, declared as a
  floor, so it imposes no test runner on you and does not hold you back from xunit 4.

## Diagnostics

Testcontainers' own log output is forwarded to xunit's diagnostic messages, which is where
a container that refuses to start explains itself. Switch them on in `xunit.runner.json`:

```json
{ "diagnosticMessages": true }
```

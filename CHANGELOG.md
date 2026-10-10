# Changelog

Every package in this repository shares one version, bumped once per release, so an entry
below covers all of them and names what changed in each. A package with nothing to report
is still republished at the new version — that is what lockstep means, and a package that
others depend on has to be there for their dependency floor to resolve.

Dates are release dates. Versions are the tags `<package-id-lowercase>/v<version>`.

## 0.3.1 — 2026-10-10

**Creomobile.Testing.Postgres.Xunit moves two dependency floors**, which is what a consumer
actually feels: taking 0.3.1 means resolving at least these versions.

- `xunit.v3.extensibility.core` 4.0.1 to 4.0.2. A bug-fix release: tests marked non-parallel
  could run in parallel and parallel tests scheduled after them could stay sequential, a
  thread-count multiplier could round down to zero and lift the limit, `SkipWhen` /
  `SkipUnless` were ignored on `InlineData`, and `IAsyncLifetime`'s documentation now states
  that `DisposeAsync` is not called when `InitializeAsync` throws — the contract this
  package's fixture already cleans up under. xunit binds its own packages to one exact
  version, so a test project on `4.0.1` moves to `4.0.2` with this package.
- `Testcontainers.PostgreSql` 4.15.0 to 4.16.0. `WithSsl` is now a method of every builder
  and works on Alpine images and under non-root users; image builds gained BuildKit and
  `--platform` variables. None of it is on the path of a plain container started from an
  image, which is all this package does.

No code in the package changed. Its README now says xunit `4.0.2` or newer.

**Creomobile.Data.Abstractions, Creomobile.Data.EFCore.CamelCaseColumns,
Creomobile.Data.EFCore.Timestamps — no change.** Republished at the lockstep version;
`Creomobile.Data.EFCore.Timestamps` requires `Creomobile.Data.Abstractions` at `>= 0.3.1`.

## 0.3.0 — 2026-10-03

**Creomobile.Testing.Postgres.Xunit requires xunit 4.** The `xunit.v3.extensibility.core`
dependency floor moves from `3.2.2` to `4.0.1`, which is why this is a minor rather than a
patch: a test project still on xunit `3.2.x` can no longer install the package, because xunit
binds its own packages to one exact version and the two requirements cannot both hold. The
floor had been kept at `3.2.2` so that the one package would serve both lines. That line has
had no release since xunit 4 shipped on 2026-08-15, and holding the floor cost something real:
the package could use only the APIs the two lines had in common, and nothing here ever ran it
on xunit 3 to prove that bet. A project already on xunit 4 resolved `4.0.x` before this release
and sees no change. No code in the package changed.

**Creomobile.Data.Abstractions, Creomobile.Data.EFCore.CamelCaseColumns,
Creomobile.Data.EFCore.Timestamps — no change.** Republished at the lockstep version;
`Creomobile.Data.EFCore.Timestamps` requires `Creomobile.Data.Abstractions` at `>= 0.3.0`.

## 0.2.7 — 2026-09-19

**The three packages that have dependencies move to the .NET 10.0.12 servicing release and
to Testcontainers 4.15.0.** These are dependency floors, which is what a consumer actually
feels: taking 0.2.7 means resolving at least these versions.

- `Creomobile.Data.EFCore.CamelCaseColumns` and `Creomobile.Data.EFCore.Timestamps` —
  `Microsoft.EntityFrameworkCore.Relational` 10.0.11 to 10.0.12.
- `Creomobile.Testing.Postgres.Xunit` — `Microsoft.Extensions.Logging.Abstractions` 10.0.11
  to 10.0.12, and `Testcontainers.PostgreSql` 4.14.0 to 4.15.0. The latter now retries a
  transient Docker image pull failure rather than letting it fail the test run, which is the
  one change in this release a consumer may notice while tests are running.

**`xunit.v3.extensibility.core` stays at 3.2.2**, as it must. It is the floor
`Creomobile.Testing.Postgres.Xunit` compiles against, and raising it would force xunit 4 on
everyone who installs that package — the single thing the pin exists to avoid.

**0.2.6 was never published.** Its tag exists and its build refused: restore failed with
NU1403, `the package is different than the last restore`. The lock files had been
regenerated on a machine whose .NET comes from the distribution's archive, and the SDK
registers its own `library-packs` folder as a restore source — so
`Microsoft.NET.ILLink.Tasks` came from the installed SDK rather than from nuget.org.
Canonical builds .NET from source and ships its own build of that package, CI installs
Microsoft's SDK and sees different bytes under the same version number, and a lock file
records the hash of whichever copy was used. This repository now takes that package from
the feed, so the hash no longer depends on who built the SDK a machine happens to run.
A tag names a commit permanently and is not moved, so the number is skipped rather than
reused. **None of this changes what the packages contain.**

**Creomobile.Data.Abstractions — no change.** It has no package dependencies at all.
Republished at the lockstep version; `Creomobile.Data.EFCore.Timestamps` requires
`Creomobile.Data.Abstractions` at `>= 0.2.7`.

## 0.2.5 — 2026-08-16

**No change to any package.** The code in all four is the same as in 0.2.3. This release
exists to carry the publishing path itself: from here on the packages are built and
published by GitHub Actions, started by a tag, and no API key is involved anywhere —
nuget.org issues the workflow a credential that lives one hour and is used once.

**0.2.4 was never published.** Its tag exists and its build refused, because the entry you
are reading now was missing at the time. A tag names a commit permanently and is not moved,
so the number was left behind rather than reused.

Republished at the lockstep version; `Creomobile.Data.EFCore.Timestamps` requires
`Creomobile.Data.Abstractions` at `>= 0.2.5`.

## 0.2.3 — 2026-08-16

**Creomobile.Data.Abstractions targets .NET 10 only again.** 0.2.2 added a `net8.0`
assembly beside it. The code was fine; the reason was not. That target was justified as
reaching "the current long-term release", and .NET 8 is not it: .NET 8 leaves support on
2026-11-10, while .NET 10 — already the only target — is the current long-term release.
Nobody had asked for .NET 8.

Removing a target framework is a breaking change, and this is one: a project on .NET 8
that took 0.2.2 cannot restore 0.2.3. It is done now because now is the day after 0.2.2
shipped, which is the narrowest this window will ever be. 0.2.2 stays on nuget.org and
keeps working, as every published version does.

If you need these contracts on an earlier runtime, open an issue. The package has no
dependencies, so nothing in it forces a recent runtime — a target framework here is a
statement about what we build and test, not a technical floor.

**Everything else — no change.** Republished at the lockstep version;
`Creomobile.Data.EFCore.Timestamps` requires `Creomobile.Data.Abstractions` at `>= 0.2.3`.

## 0.2.2 — 2026-08-16

**Creomobile.Data.Abstractions now targets .NET 8 as well as .NET 10.** It is interfaces and
one base class with no package dependencies, so nothing in it ever needed the newer runtime —
the single target framework was inherited from the repository, not chosen. Projects on .NET 8
can use it. The three EF packages stay on .NET 10, where EF Core 10 puts them.

**Creomobile.Data.Abstractions — the documentation no longer names an implementation.** Its
XML comments and README used to send you to a specific EF Core package for `UseTimestamps()`.
A package of contracts pointing at its own implementation is a dependency inverted in prose,
and it was pointing at a package that has since been split in two. The remarks now say what is
true of the contract itself: implementing it does nothing on its own, and the persistence layer
you use is what gives it effect.

**Creomobile.Testing.Postgres.Xunit — the examples pin the image by digest.** A tag is a name
and can be republished against a different image; a digest is the image's content fingerprint
and cannot be moved, so it is what makes "the same commit ran the same server" true. The
examples show the shape, not our digest — read your own from the image your production runs.
The README also now states plainly that `Container` hands back a Testcontainers type on
purpose, so a major version of that library is a breaking change for code which touches it,
while code that stays on `GetConnectionString` is unaffected.

**All packages — `InternalsVisibleTo` is no longer granted repo-wide.** It used to open every
package to `<assembly>.UnitTests` and `<assembly>.IntegrationTests`, including three packages
with no internals a test uses and one naming convention no assembly here has ever used. Only
`Creomobile.Testing.Postgres.Xunit` grants it now, to its own test assembly.

**Creomobile.Data.EFCore.CamelCaseColumns**, **Creomobile.Data.EFCore.Timestamps** — no change.
Republished at the lockstep version; `…Timestamps` requires `Creomobile.Data.Abstractions` at
`>= 0.2.2`.

## 0.2.1 — 2026-08-15

**Creomobile.Testing.Postgres.Xunit — first release.** A PostgreSQL container for xunit
integration tests, started once per test assembly through Testcontainers, plus a helper that
points its connection string at a database of your choosing. The fixture is abstract and takes
the image as a required constructor argument: the package has no default PostgreSQL version and
never picks one for you, because which one a repository tests against is a statement about that
repository's production database. One package serves both xunit `3.2.x` and `4.x` — it uses only
`IAsyncLifetime` and `AssemblyFixture`, whose shapes are identical across those lines, and it
depends on `xunit.v3.extensibility.core` alone, so it imposes no test runner.

**Creomobile.Data.EFCore.CamelCaseColumns**, **Creomobile.Data.EFCore.Timestamps** — the
`Microsoft.EntityFrameworkCore.Relational` dependency floor moves from `10.0.10` to `10.0.11`.
No API or behaviour change. Restoring either package can now pull a newer EF Core than before.

**Creomobile.Data.Abstractions** — no change. Republished because
`Creomobile.Data.EFCore.Timestamps` 0.2.1 requires it at `>= 0.2.1`.

## 0.2.0 — 2026-08-09

**Creomobile.Data.EFCore was split** into `Creomobile.Data.EFCore.CamelCaseColumns` and
`Creomobile.Data.EFCore.Timestamps`. One package that both renamed columns and ran a SaveChanges
interceptor forced every consumer to take both aspects to get either. The combined
`UseCreomobileDefaults(TimeProvider?)` is gone with it; switch the aspects on side by side:

```csharp
options
    .UseCamelCaseColumnNames()
    .UseTimestamps(timeProvider)
```

**Never reference the old package alongside the new ones.** The methods carry the same
signatures, so the compiler cannot choose between them and the build fails on an ambiguous
call. `Creomobile.Data.EFCore` 0.1.0 is deprecated on nuget.org and, like any published
package, can never be deleted.

**Creomobile.Data.Abstractions** — republished at the lockstep version; the new
`…Timestamps` requires it at `>= 0.2.0`.

## 0.1.0 — 2026-07-19

First public lockstep release of `Creomobile.Data.Abstractions` and `Creomobile.Data.EFCore`:
entity abstractions (`IEntityBase<TId>` / `EntityBase<TId>`, the UTC timestamp contracts
`ICreatedAt` / `IUpdatedAt` / `IDeletedAt` and the aggregate `ITimestamps`) and their EF Core
implementation.

## 0.0.1 – 0.0.3 — 2026-04-26 … 2026-07-18

Early releases of `Creomobile.Data.Abstractions` only, before the repository moved to a single
lockstep version.

# Changelog

Every package in this repository shares one version, bumped once per release, so an entry
below covers all of them and names what changed in each. A package with nothing to report
is still republished at the new version — that is what lockstep means, and a package that
others depend on has to be there for their dependency floor to resolve.

Dates are release dates. Versions are the tags `<package-id-lowercase>/v<version>`.

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

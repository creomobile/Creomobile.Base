# Creomobile.Base

Reusable .NET packages by Creomobile. Every package in this repository shares one
version, bumped once per release — see [CHANGELOG.md](CHANGELOG.md).

## Packages

| Package | Description |
|---|---|
| [`Creomobile.Data.Abstractions`](src/Data/Creomobile.Data.Abstractions/README.md) | Core abstractions for data models — strongly-typed entity base and timestamp marker interfaces (`ICreatedAt`, `IUpdatedAt`, `IDeletedAt`). |
| [`Creomobile.Data.EFCore.CamelCaseColumns`](src/Data/Creomobile.Data.EFCore.CamelCaseColumns/README.md) | EF Core convention that renames convention-derived column names to camelCase. Table, key and index names are left untouched. |
| [`Creomobile.Data.EFCore.Timestamps`](src/Data/Creomobile.Data.EFCore.Timestamps/README.md) | The EF Core runtime behind the timestamp contracts — automatic `CreatedAt`/`UpdatedAt`, soft delete, and a named soft-delete query filter. |
| [`Creomobile.Testing.Postgres.Xunit`](src/Testing/Creomobile.Testing.Postgres.Xunit/README.md) | A PostgreSQL container for xunit integration tests, started once per test assembly via Testcontainers. |

`Creomobile.Data.EFCore` was split into the two `EFCore.*` packages above in 0.2.0 and is
deprecated on nuget.org. Do not reference it alongside them: the extension methods carry
the same signatures, so the build fails on an ambiguous call.

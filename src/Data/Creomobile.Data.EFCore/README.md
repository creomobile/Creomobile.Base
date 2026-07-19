# Creomobile.Data.EFCore

Opinionated EF Core extensions for uniform database configuration. All
features are extension methods on `DbContextOptionsBuilder`: apply the whole
baseline with `UseCreomobileDefaults()` or opt in selectively.

## Usage

```csharp
services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseCreomobileDefaults());

public sealed class Customer : EntityBase<Guid>, ITimestamps
{
    public string Name { get; set; } = null!;
}
```

`UseCreomobileDefaults()` applies the current recommended baseline —
`UseCamelCaseColumnNames()` plus `UseTimestamps()` — and may gain new
conventions in future versions; call the individual methods instead to opt
in selectively. `EntityBase<TId>` and the timestamp interfaces come from
`Creomobile.Data.Abstractions`, installed automatically as a dependency.

## Features

### `UseCamelCaseColumnNames()`

Renames table columns whose names were derived by convention to camelCase
(`CreatedAt` → `createdAt`). Names configured explicitly — via `[Column]` or
`HasColumnName` — are left untouched; table, key, index and constraint names
are not affected.

### `UseTimestamps(TimeProvider? timeProvider = null)`

Automatically maintains the timestamp interfaces from
`Creomobile.Data.Abstractions` (`ICreatedAt`, `IUpdatedAt`, `IDeletedAt` —
each works independently, `ITimestamps` aggregates all three):

- **Insert** — `CreatedAt` and `UpdatedAt` receive the same UTC instant.
- **Update** — `UpdatedAt` is refreshed; `CreatedAt` is protected from
  accidental overwrites.
- **Delete** of an `IDeletedAt` entity becomes a **soft delete**: the row is
  kept with `DeletedAt` set, owned entities are retained; setting `DeletedAt`
  back to `null` restores the entity. Regular (non-owned) dependents still
  follow the normal delete cascade — implement `IDeletedAt` on them or adjust
  `DeleteBehavior` if they must survive the parent's soft delete.
- A named query filter (`"SoftDelete"`) hides soft-deleted entities from
  queries; bypass it per query with `IgnoreQueryFilters()` or selectively
  with `IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])`.
- All entities saved together share one instant; pass a `TimeProvider` for
  deterministic tests.

See the XML documentation of each extension method for exact semantics and
known limitations.

## Requirements

- .NET 10+
- EF Core 10

# Creomobile.Data.EFCore.Timestamps

The EF Core side of the timestamp contracts in
[`Creomobile.Data.Abstractions`](https://www.nuget.org/packages/Creomobile.Data.Abstractions):
`CreatedAt` and `UpdatedAt` are maintained for you, and deleting an entity that carries
`DeletedAt` becomes a soft delete.

## Usage

```csharp
services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseTimestamps());

public sealed class Customer : EntityBase<Guid>, ITimestamps
{
    public string Name { get; set; } = null!;
}
```

`EntityBase<TId>` and the timestamp interfaces come from `Creomobile.Data.Abstractions`,
installed automatically as a dependency.

## `UseTimestamps(TimeProvider? timeProvider = null)`

Maintains the timestamp interfaces from `Creomobile.Data.Abstractions` — `ICreatedAt`,
`IUpdatedAt`, `IDeletedAt`, each of which works independently, with `ITimestamps`
aggregating all three:

- **Insert** — `CreatedAt` and `UpdatedAt` receive the same UTC instant.
- **Update** — `UpdatedAt` is refreshed; `CreatedAt` is protected from accidental
  overwrites.
- **Delete** of an `IDeletedAt` entity becomes a **soft delete**: the row is kept with
  `DeletedAt` set, owned entities are retained; setting `DeletedAt` back to `null` restores
  the entity. Regular (non-owned) dependents still follow the normal delete cascade —
  implement `IDeletedAt` on them or adjust `DeleteBehavior` if they must survive the
  parent's soft delete.
- A named query filter (`"SoftDelete"`) hides soft-deleted entities from queries; bypass it
  per query with `IgnoreQueryFilters()` or selectively with
  `IgnoreQueryFilters([SoftDeleteQueryFilterConvention.FilterKey])`.
- All entities saved together share one instant; pass a `TimeProvider` for deterministic
  tests.

Repeated calls are no-ops: the first call wins, including its `TimeProvider`.

See the XML documentation of `UseTimestamps` and `SoftDeleteQueryFilterConvention` for exact
semantics and known limitations.

## Requirements

- .NET 10+
- EF Core 10

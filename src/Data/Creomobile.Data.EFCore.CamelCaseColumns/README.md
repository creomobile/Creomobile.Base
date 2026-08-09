# Creomobile.Data.EFCore.CamelCaseColumns

An EF Core convention that gives table columns camelCase names, and changes nothing
else in the schema.

## Usage

```csharp
services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseCamelCaseColumnNames());
```

## `UseCamelCaseColumnNames()`

Renames table columns whose names were derived by convention to camelCase
(`CreatedAt` → `createdAt`). Names configured explicitly — via `[Column]` or
`HasColumnName` — are left untouched; **table, key, index and constraint names are not
affected**.

That narrow scope is the point. General-purpose naming packages such as
`EFCore.NamingConventions` rewrite tables, keys and indexes as well, which on an existing
database means a migration that renames everything and recreates primary keys. Use this
package when you want camelCase columns and your table names to stay exactly where they
are; use `EFCore.NamingConventions` when you want a whole-schema convention.

A column shared by several properties (table splitting, owned types) is renamed only when
none of the sharing properties configures its name explicitly.

### Known limitations

TPC inheritance and entity splitting are not fully covered — properties mapped outside the
entity type's primary table keep their default names. Same-named properties on TPH sibling
types are uniquified by EF (`Type_Property`) after conventions run, so those uniquified
names escape camelCasing.

See the XML documentation of `CamelCaseColumnNamesConvention` for exact semantics.

## Requirements

- .NET 10+
- EF Core 10

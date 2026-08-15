# Creomobile.Data.Abstractions

Core abstractions for data models: entity base types and timestamp contracts.

## Overview

Provides base interfaces and classes for defining strongly-typed, persistable entities:

- **`IEntityBase<TId>`** — an entity with a strongly-typed primary key.
- **`EntityBase<TId>`** — abstract base class implementing `IEntityBase<TId>`.
- **`ICreatedAt`** — declares a UTC creation timestamp.
- **`IUpdatedAt`** — declares a UTC last-update timestamp.
- **`IDeletedAt`** — declares a nullable UTC deletion timestamp (soft delete).
- **`ITimestamps`** — combines `ICreatedAt`, `IUpdatedAt` and `IDeletedAt`.

The interfaces only declare the properties. Populating them and keeping
soft-deleted entities out of queries is the job of whatever persistence layer
recognizes the contracts — this package deliberately names none, so that an
entity model depending on it depends on no storage technology. All timestamps
are UTC — values must have `DateTimeKind.Utc`.

## Requirements

- .NET 8 or .NET 10.

This package has no dependencies of its own, so nothing here forces a recent runtime.
The other Creomobile.Data packages target .NET 10 only, because the EF Core version
they build on does.

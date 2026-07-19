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

The interfaces only declare the properties: automatic population and
soft-delete filtering are provided by the `Creomobile.Data.EFCore` package
(`UseTimestamps()`). All timestamps are UTC — values must have
`DateTimeKind.Utc`.

## Requirements

- .NET 10+

# Creomobile.Data.Abstractions

Core abstractions for Creomobile data models.

## Overview

Provides base interfaces and classes for defining strongly-typed, persistable entities:

- **`IEntityBase<TId>`** — interface for entities with a strongly-typed primary key.
- **`EntityBase<TId>`** — abstract base class implementing `IEntityBase<TId>`.
- **`ICreatedAt`** — marks an entity with a UTC creation timestamp, automatically populated on insert.
- **`IUpdatedAt`** — marks an entity with a UTC last-update timestamp, automatically maintained on insert and update.
- **`IDeletedAt`** — marks an entity supporting soft deletion semantics.
- **`ITimestamps`** — convenience aggregate of all three timestamp interfaces.

## Requirements

- .NET 10+

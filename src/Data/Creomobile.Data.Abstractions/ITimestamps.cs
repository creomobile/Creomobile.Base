namespace Creomobile.Data.Abstractions;

/// <summary>
/// Combines the creation, update and soft delete timestamp contracts.
/// </summary>
/// <remarks>
/// This interface only declares the timestamp properties. A persistence
/// integration — <c>UseTimestamps()</c> from Creomobile.Data.EFCore —
/// maintains them and applies soft-delete filtering.
/// </remarks>
public interface ITimestamps : ICreatedAt, IUpdatedAt, IDeletedAt
{
}

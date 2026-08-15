namespace Creomobile.Data.Abstractions;

/// <summary>
/// Combines the creation, update and soft delete timestamp contracts.
/// </summary>
/// <remarks>
/// This interface only declares the timestamp properties. Maintaining them and
/// applying soft-delete filtering is the job of whatever persistence layer
/// recognizes the contracts.
/// </remarks>
public interface ITimestamps : ICreatedAt, IUpdatedAt, IDeletedAt
{
}

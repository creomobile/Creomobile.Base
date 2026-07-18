namespace Creomobile.Data.Abstractions;

/// <summary>
/// Convenience aggregate interface combining creation, update and soft delete timestamp contracts.
/// Implementing this implies the entity participates in automatic timestamp population and soft delete filtering.
/// </summary>
public interface ITimestamps : ICreatedAt, IUpdatedAt, IDeletedAt
{
}

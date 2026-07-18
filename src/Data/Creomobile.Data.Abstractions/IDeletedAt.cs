namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity supporting soft deletion semantics. When deleted the entity is retained and a deletion timestamp is set.
/// </summary>
public interface IDeletedAt
{
    /// <summary>
    /// UTC deletion timestamp (stored without time zone in database) or <c>null</c> if not deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}

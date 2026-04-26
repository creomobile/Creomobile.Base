namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity supporting soft deletion semantics. When deleted the entity is retained and a deletion timestamp is set.
/// </summary>
public interface IDeletedAtTimestamp
{
    /// <summary>
    /// UTC deletion timestamp or <c>null</c> if not deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}

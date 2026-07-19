namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity supporting soft deletion semantics. When deleted the entity is retained and a deletion timestamp is set.
/// </summary>
/// <remarks>
/// The interface itself carries no behavior: soft deletion and query filtering
/// are performed by a persistence integration such as <c>UseTimestamps()</c>
/// from Creomobile.Data.EFCore.
/// </remarks>
public interface IDeletedAt
{
    /// <summary>
    /// UTC deletion timestamp or <c>null</c> if not deleted. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}

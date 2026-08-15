namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity supporting soft deletion semantics. When deleted the entity is retained and a deletion timestamp is set.
/// </summary>
/// <remarks>
/// The interface carries no behavior. It declares the contract; performing the
/// soft delete and keeping deleted entities out of queries is the job of
/// whatever persistence layer recognizes it.
/// </remarks>
public interface IDeletedAt
{
    /// <summary>
    /// UTC deletion timestamp or <c>null</c> if not deleted. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}

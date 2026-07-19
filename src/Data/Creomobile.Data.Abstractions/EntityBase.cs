namespace Creomobile.Data.Abstractions;

/// <summary>
/// Abstract base class for entities with a strongly-typed primary key.
/// </summary>
/// <typeparam name="TId">
/// Primary key type (e.g. <see cref="System.Guid"/>, <see cref="int"/>, <see cref="string"/>).
/// </typeparam>
public abstract class EntityBase<TId> : IEntityBase<TId> where TId : notnull
{
    /// <inheritdoc />
    public TId Id { get; set; } = default!;
}

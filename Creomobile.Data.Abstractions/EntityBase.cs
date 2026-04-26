namespace Creomobile.Data.Abstractions;

/// <summary>
/// Concrete base implementation of <see cref="IEntityBase{TId}"/> for persisted entities with a strongly-typed primary key.
/// </summary>
/// <typeparam name="TId">
/// Primary key type (e.g. <see cref="System.Guid"/>, <see cref="int"/>, <see cref="string"/>).
/// </typeparam>
public abstract class EntityBase<TId> : IEntityBase<TId> where TId : notnull
{
    /// <summary>
    /// Primary key value. For new (not yet persisted) entities this may be the default value of <typeparamref name="TId"/>.
    /// </summary>
    public TId Id { get; set; } = default!;
}

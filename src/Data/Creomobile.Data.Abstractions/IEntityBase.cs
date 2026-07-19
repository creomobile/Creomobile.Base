namespace Creomobile.Data.Abstractions;

/// <summary>
/// Represents an entity with a strongly-typed primary key.
/// </summary>
/// <typeparam name="TId">Primary key type (e.g. <see cref="System.Guid"/>, <see cref="int"/>, <see cref="string"/>).</typeparam>
public interface IEntityBase<TId> where TId : notnull
{
    /// <summary>
    /// Primary key value. For new (not yet persisted) entities this may be the default value of <typeparamref name="TId"/>.
    /// </summary>
    TId Id { get; set; }
}

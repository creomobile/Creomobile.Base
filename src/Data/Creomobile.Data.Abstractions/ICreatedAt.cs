namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose creation instant should be automatically populated on insert.
/// </summary>
public interface ICreatedAt
{
    /// <summary>
    /// UTC creation timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime CreatedAt { get; set; }
}

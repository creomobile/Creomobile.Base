namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose creation instant should be automatically populated on insert.
/// </summary>
public interface ICreatedAt
{
    /// <summary>
    /// UTC creation timestamp (stored without time zone in database).
    /// </summary>
    DateTime CreatedAt { get; set; }
}

namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose last update instant should be automatically maintained on insert and update.
/// </summary>
public interface IUpdatedAtTimestamp
{
    /// <summary>
    /// UTC last modification timestamp (stored without time zone in database).
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose last update instant should be automatically maintained on insert and update.
/// </summary>
public interface IUpdatedAt
{
    /// <summary>
    /// UTC last modification timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

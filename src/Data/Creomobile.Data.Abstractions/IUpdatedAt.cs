namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose last update instant should be automatically maintained on insert and update.
/// </summary>
/// <remarks>
/// The interface itself carries no behavior: the timestamp is maintained by a
/// persistence integration such as <c>UseTimestamps()</c> from
/// Creomobile.Data.EFCore.
/// </remarks>
public interface IUpdatedAt
{
    /// <summary>
    /// UTC last modification timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose creation instant should be automatically populated on insert.
/// </summary>
/// <remarks>
/// The interface itself carries no behavior: the timestamp is populated by a
/// persistence integration such as <c>UseTimestamps()</c> from
/// Creomobile.Data.EFCore.
/// </remarks>
public interface ICreatedAt
{
    /// <summary>
    /// UTC creation timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime CreatedAt { get; set; }
}

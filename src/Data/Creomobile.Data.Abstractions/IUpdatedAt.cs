namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose last update instant should be automatically maintained on insert and update.
/// </summary>
/// <remarks>
/// The interface carries no behavior. It declares the contract; maintaining the
/// value on insert and update is the job of whatever persistence layer
/// recognizes it.
/// </remarks>
public interface IUpdatedAt
{
    /// <summary>
    /// UTC last modification timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

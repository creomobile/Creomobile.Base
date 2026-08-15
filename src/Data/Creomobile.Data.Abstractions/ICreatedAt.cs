namespace Creomobile.Data.Abstractions;

/// <summary>
/// Marks an entity whose creation instant should be automatically populated on insert.
/// </summary>
/// <remarks>
/// The interface carries no behavior. It declares the contract; populating the
/// value on insert is the job of whatever persistence layer recognizes it.
/// </remarks>
public interface ICreatedAt
{
    /// <summary>
    /// UTC creation timestamp. Values must have <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime CreatedAt { get; set; }
}

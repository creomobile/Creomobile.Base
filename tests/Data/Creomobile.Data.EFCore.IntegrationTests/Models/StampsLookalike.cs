namespace Creomobile.Data.EFCore.IntegrationTests.Models;

/// <summary>
/// Control model: carries the same-named timestamp properties but implements none
/// of the marker interfaces — <c>UseTimestamps</c> must not touch it.
/// </summary>
public sealed class StampsLookalike
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

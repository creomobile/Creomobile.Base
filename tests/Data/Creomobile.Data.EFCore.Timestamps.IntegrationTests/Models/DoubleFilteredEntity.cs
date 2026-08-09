using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

/// <summary>
/// Carries its own named query filter (by <see cref="Category" />) on top of the
/// convention's soft-delete filter — proves the two coexist and that ignoring one
/// keeps the other.
/// </summary>
public sealed class DoubleFilteredEntity : IDeletedAt
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Category { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}

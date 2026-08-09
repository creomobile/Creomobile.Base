using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

public sealed class CascadeParent : IDeletedAt
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Label { get; set; } = null!;

    public List<CascadeChild> Children { get; set; } = [];

    public DateTime? DeletedAt { get; set; }
}

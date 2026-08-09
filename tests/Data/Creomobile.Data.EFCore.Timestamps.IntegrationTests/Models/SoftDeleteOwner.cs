using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

public sealed class SoftDeleteOwner : ITimestamps
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Title { get; set; } = null!;

    public OwnedDetails Details { get; set; } = null!;

    public List<OwnedTag> Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

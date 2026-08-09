using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

public sealed class UpdatedOnlyEntity : IUpdatedAt
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}

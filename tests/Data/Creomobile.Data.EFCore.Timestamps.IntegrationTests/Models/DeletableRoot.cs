using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

public abstract class DeletableRoot : EntityBase<int>, ITimestamps
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}

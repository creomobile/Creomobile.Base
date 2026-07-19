using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.IntegrationTests.Models;

public sealed class CreatedOnlyEntity : ICreatedAt
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}

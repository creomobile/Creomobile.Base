namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

public sealed class DeletableLeaf : DeletableRoot
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Extra { get; set; }
}

namespace Creomobile.Data.EFCore.IntegrationTests.Models;

public sealed class OwnedTag
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Label { get; set; } = null!;
}

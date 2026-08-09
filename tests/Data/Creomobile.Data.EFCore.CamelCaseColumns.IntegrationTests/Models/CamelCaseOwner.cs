namespace Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests.Models;

public sealed class CamelCaseOwner
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Name { get; set; } = null!;

    public CamelCaseOwnerDetails Details { get; set; } = null!;
}

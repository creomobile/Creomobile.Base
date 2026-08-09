namespace Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests.Models;

public class TphAnimal
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Name { get; set; } = null!;
}

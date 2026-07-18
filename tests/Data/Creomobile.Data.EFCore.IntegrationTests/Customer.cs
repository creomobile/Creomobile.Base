using System.ComponentModel.DataAnnotations.Schema;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class Customer
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Name { get; set; } = null!;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column("LegacyName")]
    public string? ExplicitlyNamed { get; set; }
}

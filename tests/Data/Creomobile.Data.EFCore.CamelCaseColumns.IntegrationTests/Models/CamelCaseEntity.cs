using System.ComponentModel.DataAnnotations.Schema;

namespace Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests.Models;

public sealed class CamelCaseEntity
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string ConventionNamed { get; set; } = null!;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column("LegacyName")]
    public string? ExplicitlyNamed { get; set; }
}

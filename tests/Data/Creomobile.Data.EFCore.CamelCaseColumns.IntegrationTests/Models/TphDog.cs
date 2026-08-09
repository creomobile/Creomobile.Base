using System.ComponentModel.DataAnnotations.Schema;

namespace Creomobile.Data.EFCore.CamelCaseColumns.IntegrationTests.Models;

public sealed class TphDog : TphAnimal
{
    // Explicitly configured to its own default name: at convention time this
    // protects the name group shared with TphCat.SharedTrait from renaming.
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    [Column("SharedTrait")]
    public string? SharedTrait { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Fur { get; set; }
}

namespace Creomobile.Data.EFCore.IntegrationTests.Models;

public sealed class TphCat : TphAnimal
{
    // Same default name as TphDog.SharedTrait, which is explicit — at
    // convention time the group is protected; EF then uniquifies this side.
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? SharedTrait { get; set; }

    // Same default name as TphDog.Fur, both by convention — renamed as a
    // group, then EF uniquifies the dog's side.
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Fur { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? CatOnly { get; set; }
}

namespace Creomobile.Data.EFCore.IntegrationTests.Models;

public sealed class CascadeChild
{
    public int Id { get; set; }

    // Required FK: deleting the parent cascades to tracked children.
    public int CascadeParentId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Label { get; set; } = null!;
}

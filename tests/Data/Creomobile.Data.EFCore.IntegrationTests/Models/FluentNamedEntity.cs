namespace Creomobile.Data.EFCore.IntegrationTests.Models;

/// <summary>
/// Its column name is configured fluently (<c>HasColumnName</c>) in the context —
/// the convention must leave it untouched, same as a <c>[Column]</c> attribute.
/// </summary>
public sealed class FluentNamedEntity
{
    public int Id { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? FluentNamed { get; set; }
}

using Creomobile.Data.Abstractions;

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;

/// <summary>
/// Implements <see cref="IDeletedAt"/> <b>explicitly</b>, so the member is private and named
/// <c>Creomobile.Data.Abstractions.IDeletedAt.DeletedAt</c>. EF discovers no CLR property called
/// <c>DeletedAt</c>, and mapping one anyway produces a shadow property — which is the only way
/// to reach the <c>EF.Property</c> branch of the soft-delete filter builder.
/// </summary>
public sealed class ShadowDeletedAtEntity : EntityBase<int>, IDeletedAt
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Payload { get; set; } = null!;

    DateTime? IDeletedAt.DeletedAt { get; set; }
}

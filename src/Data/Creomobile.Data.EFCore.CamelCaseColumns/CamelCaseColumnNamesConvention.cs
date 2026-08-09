using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Creomobile.Data.EFCore.CamelCaseColumns;

/// <summary>
/// Renames table columns whose names were derived by convention to camelCase.
/// Column names configured explicitly (via <c>[Column]</c> or <c>HasColumnName</c>)
/// are left untouched; a column shared by several properties (table splitting,
/// owned types) is renamed only when none of the sharing properties configures
/// its name explicitly. Table, key, index and constraint names are not affected.
/// </summary>
/// <remarks>
/// Known limitations: TPC inheritance and entity splitting are not fully
/// covered — properties mapped outside the entity type's primary table keep
/// their default names. Same-named properties on TPH sibling types are
/// uniquified by EF (<c>Type_Property</c>) after conventions run, so those
/// uniquified names escape camelCasing. The convention runs as a
/// model-finalizing convention after the built-in ones; its order relative
/// to other convention-set plugins follows the plugin registration order.
/// </remarks>
public sealed class CamelCaseColumnNamesConvention : IModelFinalizingConvention
{
    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        // A physical column can back several properties (table splitting, owned
        // types sharing the owner's table). Renaming one side alone would leave
        // the shared column with mismatched names and fail model validation, so
        // properties are grouped per (table, column) and renamed as a unit.
        // ToList keeps the phases strictly separate: all names are read before
        // the first rename is applied.
        var renames = (
            from entityType in modelBuilder.Metadata.GetEntityTypes()
            let table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)
            where table is not null
            from property in entityType.GetDeclaredProperties()
            let columnName = property.GetColumnName(table.Value)
            where columnName is not null
            group property by (Table: table.Value, Column: columnName) into sharers
            where !sharers.Any(p => IsExplicitlyNamed(p, sharers.Key.Table))
            let camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(sharers.Key.Column)
            where camelCaseName != sharers.Key.Column
            from property in sharers
            select (property, camelCaseName)).ToList();

        foreach (var (property, camelCaseName) in renames)
            property.Builder.HasColumnName(camelCaseName);
    }

    static bool IsExplicitlyNamed(IConventionProperty property, in StoreObjectIdentifier table)
    {
        // Only developer-made choices are protected; a name supplied by another
        // convention is still eligible for camelCasing.
        var source = property.GetColumnNameConfigurationSource();
        if (source is ConfigurationSource.Explicit or ConfigurationSource.DataAnnotation)
            return true;

        var overrideSource = property.FindOverrides(table)?.GetColumnNameConfigurationSource();
        return overrideSource is ConfigurationSource.Explicit or ConfigurationSource.DataAnnotation;
    }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Creomobile.Data.EFCore;

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
/// their default names. The convention runs as a model-finalizing convention
/// after the built-in ones; its order relative to other convention-set plugins
/// follows the plugin registration order.
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
        var columns = new Dictionary<(StoreObjectIdentifier Table, string Column), List<IConventionProperty>>();

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
            if (table is null)
                continue;

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var columnName = property.GetColumnName(table.Value);
                if (columnName is null)
                    continue;

                var key = (table.Value, columnName);
                if (!columns.TryGetValue(key, out var group))
                    columns[key] = group = [];
                group.Add(property);
            }
        }

        foreach (var ((table, columnName), properties) in columns)
        {
            if (properties.Any(p => IsExplicitlyNamed(p, table)))
                continue;

            var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(columnName);
            if (camelCaseName == columnName)
                continue;

            foreach (var property in properties)
                property.Builder.HasColumnName(camelCaseName);
        }
    }

    private static bool IsExplicitlyNamed(IConventionProperty property, in StoreObjectIdentifier table)
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

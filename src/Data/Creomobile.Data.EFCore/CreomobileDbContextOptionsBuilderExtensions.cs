using Creomobile.Data.EFCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Creomobile extensions for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class CreomobileDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Renames table columns whose names were derived by convention to camelCase.
    /// Column names configured explicitly (via <c>[Column]</c> or <c>HasColumnName</c>)
    /// are left untouched. Table, key, index and constraint names are not affected.
    /// </summary>
    /// <remarks>
    /// See <see cref="CamelCaseColumnNamesConvention" /> for exact semantics and
    /// known limitations.
    /// </remarks>
    public static DbContextOptionsBuilder UseCamelCaseColumnNames(
        this DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<CamelCaseColumnNamesOptionsExtension>()
                        ?? new CamelCaseColumnNamesOptionsExtension();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return optionsBuilder;
    }

    /// <inheritdoc cref="UseCamelCaseColumnNames(DbContextOptionsBuilder)" />
    public static DbContextOptionsBuilder<TContext> UseCamelCaseColumnNames<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseCamelCaseColumnNames((DbContextOptionsBuilder)optionsBuilder);
}

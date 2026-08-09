using Creomobile.Data.EFCore.CamelCaseColumns;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// camelCase column naming extensions for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class CamelCaseColumnsDbContextOptionsBuilderExtensions
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
    /// <param name="optionsBuilder">The options builder to configure.</param>
    /// <returns>The same builder instance so that calls can be chained.</returns>
    public static DbContextOptionsBuilder UseCamelCaseColumnNames(
        this DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<CamelCaseColumnNamesOptionsExtension>()
                        ?? new CamelCaseColumnNamesOptionsExtension();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return optionsBuilder;
    }

    /// <inheritdoc cref="UseCamelCaseColumnNames(DbContextOptionsBuilder)" />
    /// <typeparam name="TContext">The <see cref="DbContext" /> type being configured.</typeparam>
    public static DbContextOptionsBuilder<TContext> UseCamelCaseColumnNames<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)((DbContextOptionsBuilder)optionsBuilder).UseCamelCaseColumnNames();
}

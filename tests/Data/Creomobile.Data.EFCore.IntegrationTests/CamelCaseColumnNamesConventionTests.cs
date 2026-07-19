using Microsoft.EntityFrameworkCore;

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CamelCaseColumnNamesConventionTests(PostgresFixture postgresFixture)
{
    const string Database = "camel_case_tests";

    [Fact]
    public async Task CreatesCamelCaseColumnsForDefaultNamesOnly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var columns = await GetColumnsAsync("CamelCaseEntities", cancellationToken);

        columns.Should().BeEquivalentTo("id", "conventionNamed", "LegacyName");
    }

    [Fact]
    public async Task LeavesFluentlyConfiguredColumnNamesUntouched()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var columns = await GetColumnsAsync("FluentNamedEntities", cancellationToken);

        columns.Should().BeEquivalentTo("id", "FluentLegacy");
    }

    [Fact]
    public async Task RenamesTphColumnsAndProtectsSharedGroupsWithAnExplicitMember()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var columns = await GetColumnsAsync("TphAnimals", cancellationToken);

        // Root and derived-only properties (including the discriminator) are
        // camelCased. Same-named sibling properties do NOT share a column:
        // EF uniquifies them AFTER conventions run, so their prefixed names
        // escape camelCasing — a documented limitation of the convention.
        // TphDog.SharedTrait keeps its explicit [Column] name; the group it
        // formed with TphCat.SharedTrait at convention time was protected,
        // and the cat's side was then uniquified to "TphCat_SharedTrait".
        // The convention-named "Fur" pair was renamed to "fur" as a group,
        // after which EF uniquified the dog's side to "TphDog_Fur".
        columns.Should().BeEquivalentTo(
            "id", "name", "discriminator", "catOnly",
            "SharedTrait", "TphCat_SharedTrait", "fur", "TphDog_Fur");
    }

    [Fact]
    public async Task RenamesColumnsSharedByOwnerAndOwnedTypeAsAUnit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var columns = await GetColumnsAsync("CamelCaseOwners", cancellationToken);

        // Table splitting: the owner's PK column is shared with the owned
        // entry's key — renaming must happen for both properties at once or
        // model validation would fail before any SQL runs.
        columns.Should().BeEquivalentTo("id", "name", "details_City");
    }

    async Task<List<string>> GetColumnsAsync(string tableName, CancellationToken cancellationToken)
    {
        var connectionString = TestDatabase.ConnectionString(postgresFixture, Database);

        var options = new DbContextOptionsBuilder<CamelCaseTestContext>()
            .UseNpgsql(connectionString)
            .UseCamelCaseColumnNames()
            .Options;

        await using var context = new CamelCaseTestContext(options);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        return await TestDatabase.GetTableColumnsAsync(connectionString, tableName, cancellationToken);
    }
}

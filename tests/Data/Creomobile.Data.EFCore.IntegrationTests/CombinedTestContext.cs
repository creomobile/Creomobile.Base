using Creomobile.Data.EFCore.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CombinedTestContext(DbContextOptions<CombinedTestContext> options)
    : DbContext(options)
{
    public DbSet<CombinedEntity> CombinedEntities => Set<CombinedEntity>();
}

using Creomobile.Data.EFCore.Timestamps.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.Timestamps.IntegrationTests;

public sealed class CombinedTestContext(DbContextOptions<CombinedTestContext> options)
    : DbContext(options)
{
    public DbSet<CombinedEntity> CombinedEntities => Set<CombinedEntity>();
}

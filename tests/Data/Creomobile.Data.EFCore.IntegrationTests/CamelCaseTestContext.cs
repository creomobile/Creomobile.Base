using Creomobile.Data.EFCore.IntegrationTests.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class CamelCaseTestContext(DbContextOptions<CamelCaseTestContext> options)
    : DbContext(options)
{
    public DbSet<CamelCaseEntity> CamelCaseEntities => Set<CamelCaseEntity>();
}

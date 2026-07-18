using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMember.Global

namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class ConventionTestContext(DbContextOptions<ConventionTestContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
}

using Microsoft.EntityFrameworkCore;

using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence.Convensions;

namespace Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new StringMaxLengthConversion(100));
    }
}

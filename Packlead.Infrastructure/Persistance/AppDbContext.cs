using Microsoft.EntityFrameworkCore;
using Packlead.Domain.Entities;
using Packlead.Infrastructure.Persistence.Configurations;

namespace Packlead.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Dispatcher> Dispatchers => Set<Dispatcher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new DispatcherConfiguration());
    }
}
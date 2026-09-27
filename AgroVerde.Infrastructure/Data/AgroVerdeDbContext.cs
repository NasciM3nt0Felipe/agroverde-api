using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Data;

public class AgroVerdeDbContext : DbContext
{
    public AgroVerdeDbContext(
        DbContextOptions<AgroVerdeDbContext> options)
        : base(options)
    {
    }

public DbSet<Talhao> Talhoes => Set<Talhao>();
public DbSet<Safra> Safras => Set<Safra>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AgroVerdeDbContext).Assembly
        );
    }
}
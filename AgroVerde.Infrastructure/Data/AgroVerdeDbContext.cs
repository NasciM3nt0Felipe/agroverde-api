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
    public DbSet<EstoqueItem> EstoqueItens => Set<EstoqueItem>();
    public DbSet<EstoqueInsumo> EstoqueInsumos => Set<EstoqueInsumo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AgroVerdeDbContext).Assembly
        );
    }
}
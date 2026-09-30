using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Data;

public class AgroVerdeDbContext : DbContext
{
    public AgroVerdeDbContext(DbContextOptions<AgroVerdeDbContext> options) : base(options)
    {
    }

    public DbSet<Talhao> Talhoes { get; set; }

    // Módulos de Rebanho e Financeiro
    public DbSet<Animal> Animais { get; set; }
    public DbSet<Pesagem> Pesagens { get; set; }
    public DbSet<Vacinacao> Vacinacoes { get; set; }
    public DbSet<ControleSanitario> ControlesSanitarios { get; set; }
    public DbSet<RegistroReproducao> RegistrosReproducao { get; set; }
    public DbSet<TransacaoFinanceira> TransacoesFinanceiras { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; } = null!;
    public DbSet<Pessoa> Pessoas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgroVerdeDbContext).Assembly);
    }
}
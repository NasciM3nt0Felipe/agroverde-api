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

    // Talhão
    public DbSet<Talhao> Talhoes => Set<Talhao>();

    // Safra
    public DbSet<Safra> Safras => Set<Safra>();

    // Estoque
    public DbSet<EstoqueItem> EstoqueItens => Set<EstoqueItem>();
    public DbSet<EstoqueInsumo> EstoqueInsumos => Set<EstoqueInsumo>();

    // Usuário e Propriedade
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Propriedade> Propriedades => Set<Propriedade>();

    // Rebanho
    public DbSet<Animal> Animais => Set<Animal>();
    public DbSet<Pesagem> Pesagens => Set<Pesagem>();
    public DbSet<Vacinacao> Vacinacoes => Set<Vacinacao>();
    public DbSet<ControleSanitario> ControlesSanitarios => Set<ControleSanitario>();
    public DbSet<RegistroReproducao> RegistrosReproducao => Set<RegistroReproducao>();

    // Financeiro
    public DbSet<TransacaoFinanceira> TransacoesFinanceiras => Set<TransacaoFinanceira>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AgroVerdeDbContext).Assembly
        );
    }
}
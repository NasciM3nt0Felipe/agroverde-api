using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations
{
    public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
    {
        public void Configure(EntityTypeBuilder<Veiculo> builder)
        {
            builder.ToTable("Veiculos");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Nome).IsRequired().HasMaxLength(150);
            builder.Property(v => v.PlacaOuChassi).HasMaxLength(50);
            builder.Property(v => v.Marca).HasMaxLength(100);
            builder.Property(v => v.Modelo).HasMaxLength(100);
            builder.Property(v => v.Observacoes).HasMaxLength(500);

            builder.HasIndex(v => v.PropriedadeId);

            // Placa/chassi única dentro da propriedade.
            builder.HasIndex(v => new { v.PropriedadeId, v.PlacaOuChassi })
                .IsUnique()
                .HasFilter("\"PlacaOuChassi\" IS NOT NULL");

            // Veículo com abastecimentos ou manutenções não pode ser excluído
            // (o histórico e as despesas no Financeiro seriam perdidos).
            builder.HasMany(v => v.Abastecimentos)
                .WithOne(a => a.Veiculo)
                .HasForeignKey(a => a.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(v => v.Manutencoes)
                .WithOne(m => m.Veiculo)
                .HasForeignKey(m => m.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AbastecimentoVeiculoConfiguration : IEntityTypeConfiguration<AbastecimentoVeiculo>
    {
        public void Configure(EntityTypeBuilder<AbastecimentoVeiculo> builder)
        {
            builder.ToTable("AbastecimentosVeiculo");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.ValorTotal).HasPrecision(18, 2);
            builder.Property(a => a.Observacao).HasMaxLength(500);
        }
    }

    public class ManutencaoVeiculoConfiguration : IEntityTypeConfiguration<ManutencaoVeiculo>
    {
        public void Configure(EntityTypeBuilder<ManutencaoVeiculo> builder)
        {
            builder.ToTable("ManutencoesVeiculo");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Tipo).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Descricao).HasMaxLength(500);
            builder.Property(m => m.Valor).HasPrecision(18, 2);
            builder.Property(m => m.Observacao).HasMaxLength(500);
        }
    }
}

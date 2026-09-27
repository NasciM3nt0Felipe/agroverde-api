using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class SafraConfiguration : IEntityTypeConfiguration<Safra>
{
    public void Configure(EntityTypeBuilder<Safra> builder)
    {
        builder.ToTable("safra");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.TalhaoId)
            .HasColumnName("talhao_id")
            .IsRequired();

        builder.Property(s => s.Nome)
            .HasColumnName("nome")
            .IsRequired();

        builder.Property(s => s.Cultura)
            .HasColumnName("cultura")
            .IsRequired();

        builder.Property(s => s.Variedade)
            .HasColumnName("variedade");

        builder.Property(s => s.DataPlantio)
            .HasColumnName("data_plantio")
            .IsRequired();

        builder.Property(s => s.DataColheitaPrevista)
            .HasColumnName("data_colheita_prevista");

        builder.Property(s => s.DataColheitaReal)
            .HasColumnName("data_colheita_real");

        builder.Property(s => s.ProducaoEstimada)
            .HasColumnName("producao_estimada");

        builder.Property(s => s.ProducaoObtida)
            .HasColumnName("producao_obtida");

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(s => s.Observacao)
            .HasColumnName("observacao");
    }
}
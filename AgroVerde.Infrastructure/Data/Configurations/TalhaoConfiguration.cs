using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class TalhaoConfiguration : IEntityTypeConfiguration<Talhao>
{
    public void Configure(EntityTypeBuilder<Talhao> builder)
    {
        builder.ToTable("talhao");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.PropriedadeId)
            .HasColumnName("propriedade_id")
            .IsRequired();

        builder.Property(t => t.Nome)
            .HasColumnName("nome")
            .IsRequired();

        builder.Property(t => t.Area)
            .HasColumnName("area")
            .IsRequired();

        builder.Property(t => t.TipoSolo)
            .HasColumnName("tipo_solo");

        builder.Property(t => t.Observacao)
            .HasColumnName("observacao");

        builder.Property(t => t.Ativo)
            .HasColumnName("ativo")
            .IsRequired();
    }
}
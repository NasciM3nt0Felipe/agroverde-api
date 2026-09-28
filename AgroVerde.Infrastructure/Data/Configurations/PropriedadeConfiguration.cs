using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class PropriedadeConfiguration : IEntityTypeConfiguration<Propriedade>
{
    public void Configure(EntityTypeBuilder<Propriedade> builder)
    {
        builder.ToTable("propriedade");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Nome)
            .HasColumnName("nome")
            .IsRequired();

        builder.Property(p => p.Localizacao)
            .HasColumnName("localizacao")
            .IsRequired();

        builder.Property(p => p.AreaTotal)
            .HasColumnName("area_total")
            .IsRequired();

        builder.Property(p => p.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();
    }
}

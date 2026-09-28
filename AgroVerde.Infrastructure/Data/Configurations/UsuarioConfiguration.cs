using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.SenhaHash)
            .HasColumnName("senha_hash")
            .IsRequired();

        builder.Property(u => u.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();
    }
}

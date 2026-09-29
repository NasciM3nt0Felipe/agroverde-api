using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Identificacao)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Especie)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Raca)
            .HasMaxLength(100);

        builder.Property(a => a.Sexo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Observacao)
            .HasMaxLength(500);

        builder.HasMany(a => a.Pesagens)
            .WithOne(p => p.Animal)
            .HasForeignKey(p => p.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Vacinacoes)
            .WithOne(v => v.Animal)
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ControlesSanitarios)
            .WithOne(c => c.Animal)
            .HasForeignKey(c => c.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Reproducoes)
            .WithOne(r => r.Animal)
            .HasForeignKey(r => r.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PesagemConfiguration : IEntityTypeConfiguration<Pesagem>
{
    public void Configure(EntityTypeBuilder<Pesagem> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PesoKg)
            .HasPrecision(8, 2)
            .IsRequired();

        builder.Property(p => p.Observacao)
            .HasMaxLength(500);
    }
}
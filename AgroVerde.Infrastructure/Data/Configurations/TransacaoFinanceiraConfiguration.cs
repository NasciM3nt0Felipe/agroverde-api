using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class TransacaoFinanceiraConfiguration : IEntityTypeConfiguration<TransacaoFinanceira>
{
    public void Configure(EntityTypeBuilder<TransacaoFinanceira> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Categoria)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        // Relacionamento opcional com Animal
        builder.HasOne(t => t.Animal)
            .WithMany()
            .HasForeignKey(t => t.AnimalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
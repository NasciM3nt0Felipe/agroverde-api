using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class EstoqueInsumoConfiguration : IEntityTypeConfiguration<EstoqueInsumo>
{
    public void Configure(EntityTypeBuilder<EstoqueInsumo> builder)
    {
        builder.ToTable("estoque_insumo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.SafraId)
            .HasColumnName("safra_id")
            .IsRequired();

        builder.Property(e => e.EstoqueItemId)
            .HasColumnName("estoque_item_id")
            .IsRequired();

        builder.Property(e => e.QuantidadeUtilizada)
            .HasColumnName("quantidade_utilizada")
            .IsRequired();

        builder.Property(e => e.ValorTotal)
            .HasColumnName("valor_total")
            .IsRequired();

        builder.Property(e => e.DataMovimentacao)
            .HasColumnName("data_movimentacao")
            .IsRequired();

        builder.Property(e => e.Observacao)
            .HasColumnName("observacao");
    }
}
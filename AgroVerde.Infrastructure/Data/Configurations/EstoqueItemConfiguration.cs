using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class EstoqueItemConfiguration : IEntityTypeConfiguration<EstoqueItem>
{
    public void Configure(EntityTypeBuilder<EstoqueItem> builder)
    {
        builder.ToTable("estoque_item");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.PropriedadeId)
            .HasColumnName("propriedade_id")
            .IsRequired();

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .IsRequired();

        builder.Property(e => e.Categoria)
            .HasColumnName("categoria")
            .IsRequired();

        builder.Property(e => e.QuantidadeInicial)
            .HasColumnName("quantidade_inicial")
            .IsRequired();

        builder.Property(e => e.QuantidadeAtual)
            .HasColumnName("quantidade_atual")
            .IsRequired();

        builder.Property(e => e.UnidadeMedida)
            .HasColumnName("unidade_medida")
            .IsRequired();

        builder.Property(e => e.PrecoMedioUnitario)
            .HasColumnName("preco_medio_unitario")
            .IsRequired();

        builder.Property(e => e.EstoqueMinimo)
            .HasColumnName("estoque_minimo")
            .IsRequired();

        builder.Property(e => e.Fornecedor)
            .HasColumnName("fornecedor");

        builder.Property(e => e.Observacao)
            .HasColumnName("observacao");

        builder.Ignore(e => e.ValorTotal);
        builder.Ignore(e => e.EstoqueZerado);
        builder.Ignore(e => e.EstoqueBaixo);
    }
}
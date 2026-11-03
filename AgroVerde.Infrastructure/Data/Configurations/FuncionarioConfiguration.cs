using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionario");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.PessoaId)
            .HasColumnName("pessoa_id")
            .IsRequired();

        builder.Property(f => f.PropriedadeId)
            .HasColumnName("propriedade_id")
            .IsRequired();

        builder.Property(f => f.Cargo)
            .HasColumnName("cargo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Salario)
            .HasColumnName("salario")
            .HasPrecision(18, 2);

        builder.Property(f => f.DataContratacao).HasColumnName("data_contratacao");

        builder.Property(f => f.DataDesligamento).HasColumnName("data_desligamento");

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(500);

        // Sem cascata: excluir Pessoa ou Propriedade com funcionário vinculado é barrado pelo banco.
        builder.HasOne(f => f.Pessoa)
            .WithMany()
            .HasForeignKey(f => f.PessoaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Propriedade>()
            .WithMany()
            .HasForeignKey(f => f.PropriedadeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Uma pessoa só pode ser funcionária uma vez por propriedade.
        builder.HasIndex(f => new { f.PessoaId, f.PropriedadeId }).IsUnique();

        builder.HasIndex(f => f.PropriedadeId);
    }
}

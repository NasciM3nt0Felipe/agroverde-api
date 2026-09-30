using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations
{
    public class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
    {
        public void Configure(EntityTypeBuilder<Pessoa> builder)
        {
            builder.ToTable("Pessoas");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
            builder.Property(p => p.CpfCnpj).HasMaxLength(20);
            builder.Property(p => p.Telefone).HasMaxLength(20);
            builder.Property(p => p.Email).HasMaxLength(100);
            builder.Property(p => p.CargoOuFuncao).HasMaxLength(100);
            builder.Property(p => p.Observacoes).HasMaxLength(500);

            builder.HasIndex(p => p.CpfCnpj).IsUnique().HasFilter("[CpfCnpj] IS NOT NULL");
        }
    }
}
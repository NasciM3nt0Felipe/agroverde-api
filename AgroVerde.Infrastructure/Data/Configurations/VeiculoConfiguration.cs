using AgroVerde.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroVerde.Infrastructure.Data.Configurations
{
    public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
    {
        public void Configure(EntityTypeBuilder<Veiculo> builder)
        {
            builder.ToTable("Veiculos");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Nome).IsRequired().HasMaxLength(150);
            builder.Property(v => v.PlacaOuChassi).HasMaxLength(50);
            builder.Property(v => v.Modelo).HasMaxLength(100);
            builder.Property(v => v.Observacoes).HasMaxLength(500);

            builder.HasIndex(v => v.PlacaOuChassi).IsUnique().HasFilter("[PlacaOuChassi] IS NOT NULL");
        }
    }
}
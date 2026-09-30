using System;

namespace AgroVerde.Domain.Entities
{
    public enum TipoVeiculo
    {
        Trator = 1,
        Colheitadeira = 2,
        Pulverizador = 3,
        Caminhonete = 4,
        Caminhao = 5,
        Implemento = 6,
        Outro = 7
    }

    public enum StatusVeiculo
    {
        Ativo = 1,
        EmManutencao = 2,
        Inativo = 3
    }

    public class Veiculo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? PlacaOuChassi { get; set; }
        public TipoVeiculo Tipo { get; set; }
        public string? Modelo { get; set; }
        public int? AnoFabricacao { get; set; }
        public double HorimetroAtual { get; set; }
        public double QuilometragemAtual { get; set; }
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Ativo;
        public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;
        public string? Observacoes { get; set; }
    }
}
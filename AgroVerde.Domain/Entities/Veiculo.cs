using System;
using System.Collections.Generic;

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
        Inativo = 3,
        Vendido = 4
    }

    public class Veiculo
    {
        public int Id { get; set; }

        // Propriedade dona do veículo (isolamento por usuário).
        public int PropriedadeId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? PlacaOuChassi { get; set; }
        public TipoVeiculo Tipo { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? AnoFabricacao { get; set; }
        public double HorimetroAtual { get; set; }
        public double QuilometragemAtual { get; set; }
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Ativo;
        public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;
        public string? Observacoes { get; set; }

        public double? ValorVenda { get; set; }
        public DateTime? DataVenda { get; set; }

        public ICollection<AbastecimentoVeiculo> Abastecimentos { get; set; }
            = new List<AbastecimentoVeiculo>();

        public ICollection<ManutencaoVeiculo> Manutencoes { get; set; }
            = new List<ManutencaoVeiculo>();
    }

    // Abastecimento do veículo. A despesa no Financeiro é criada pela API
    // na mesma transação (OrigemTipo "Abastecimento", OrigemId = Id).
    public class AbastecimentoVeiculo
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;
        public DateTime Data { get; set; }
        public double Litros { get; set; }
        public decimal ValorTotal { get; set; }
        public double? HorimetroOdometro { get; set; }
        public string? Observacao { get; set; }
    }

    // Manutenção do veículo. A despesa no Financeiro é criada pela API
    // na mesma transação (OrigemTipo "ManutencaoVeiculo", OrigemId = Id).
    public class ManutencaoVeiculo
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public double? HorimetroOdometro { get; set; }
        public string? Observacao { get; set; }
    }
}

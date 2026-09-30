using System;
using System.ComponentModel.DataAnnotations;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.DTOs
{
    public class VeiculoRequest
    {
        [Required(ErrorMessage = "O nome/identificação do veículo é obrigatório.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "A placa ou chassi deve ter no máximo 50 caracteres.")]
        public string? PlacaOuChassi { get; set; }

        [Required(ErrorMessage = "O tipo de veículo é obrigatório.")]
        [Range(1, 7, ErrorMessage = "Tipo de veículo inválido.")]
        public TipoVeiculo Tipo { get; set; }

        [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres.")]
        public string? Modelo { get; set; }

        [Range(1950, 2100, ErrorMessage = "Ano de fabricação inválido.")]
        public int? AnoFabricacao { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O horímetro não pode ser negativo.")]
        public double HorimetroAtual { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public double QuilometragemAtual { get; set; }

        [Range(1, 3, ErrorMessage = "Status do veículo inválido.")]
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Ativo;

        public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
        public string? Observacoes { get; set; }
    }

    public class VeiculoResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? PlacaOuChassi { get; set; }
        public TipoVeiculo Tipo { get; set; }
        public string TipoDescricao { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public int? AnoFabricacao { get; set; }
        public double HorimetroAtual { get; set; }
        public double QuilometragemAtual { get; set; }
        public StatusVeiculo Status { get; set; }
        public string StatusDescricao { get; set; } = string.Empty;
        public DateTime DataAquisicao { get; set; }
        public string? Observacoes { get; set; }
    }

    public class AtualizarHodometroRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "O novo horímetro não pode ser negativo.")]
        public double NovoHorimetro { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A nova quilometragem não pode ser negativa.")]
        public double NovaQuilometragem { get; set; }
    }
}
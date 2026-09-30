using System;

namespace AgroVerde.Domain.Entities
{
    public enum TipoPessoa
    {
        Funcionario = 1,
        Fornecedor = 2,
        Cliente = 3,
        Veterinario = 4,
        Agronomo = 5,
        Outro = 6
    }

    public class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? CpfCnpj { get; set; }
        public TipoPessoa Tipo { get; set; } = TipoPessoa.Funcionario;
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? CargoOuFuncao { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public string? Observacoes { get; set; }
    }
}
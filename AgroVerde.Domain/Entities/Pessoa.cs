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

        // ---- Perfil do usuário (módulo Perfil do app) ----
        // UsuarioId preenchido = esta linha é o PERFIL desse usuário (no máximo um por usuário).
        // UsuarioId nulo = pessoa operacional (funcionário, fornecedor etc.), como sempre foi.
        // O CPF do perfil fica em CpfCnpj. Sem chave estrangeira: o vínculo é garantido pela API.
        public int? UsuarioId { get; set; }
        public string? Cep { get; set; }
        public string? Rua { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.DTOs
{
    public class PessoaRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O CPF/CNPJ deve ter no máximo 20 caracteres.")]
        public string? CpfCnpj { get; set; }

        [Required(ErrorMessage = "O tipo de pessoa é obrigatório.")]
        [Range(1, 6, ErrorMessage = "Tipo de pessoa inválido.")]
        public TipoPessoa Tipo { get; set; }

        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "O cargo/função deve ter no máximo 100 caracteres.")]
        public string? CargoOuFuncao { get; set; }

        public bool Ativo { get; set; } = true;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
        public string? Observacoes { get; set; }
    }

    public class PessoaResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? CpfCnpj { get; set; }
        public TipoPessoa Tipo { get; set; }
        public string TipoDescricao { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? CargoOuFuncao { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public string? Observacoes { get; set; }
    }
}
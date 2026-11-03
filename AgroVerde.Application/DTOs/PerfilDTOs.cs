using System.ComponentModel.DataAnnotations;

namespace AgroVerde.Application.DTOs
{
    // Perfil do usuário autenticado. O usuário NUNCA vem no corpo: é o do token JWT.
    public class PerfilRequest
    {
        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O CPF deve ter no máximo 20 caracteres.")]
        public string? Cpf { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        [StringLength(10, ErrorMessage = "O CEP deve ter no máximo 10 caracteres.")]
        public string? Cep { get; set; }

        [StringLength(150, ErrorMessage = "A rua deve ter no máximo 150 caracteres.")]
        public string? Rua { get; set; }

        [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
        public string? Numero { get; set; }

        [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
        public string? Bairro { get; set; }

        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string? Cidade { get; set; }

        [StringLength(50, ErrorMessage = "O estado deve ter no máximo 50 caracteres.")]
        public string? Estado { get; set; }
    }

    public class PerfilResponse
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Cpf { get; set; }
        public string? Telefone { get; set; }
        public string? Cep { get; set; }
        public string? Rua { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }
}

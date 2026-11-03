using System.ComponentModel.DataAnnotations;

namespace AgroVerde.Application.DTOs;

public class AlterarSenhaRequest
{
    [StringLength(128, ErrorMessage = "A senha atual deve ter no máximo 128 caracteres.")]
    public string SenhaAtual { get; set; } = string.Empty;

    [StringLength(128, ErrorMessage = "A nova senha deve ter no máximo 128 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}

using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.Services;

// Abstração usada pelo UsuarioService; a implementação (JWT) fica na camada API.
public interface ITokenService
{
    string GerarToken(Usuario usuario);
}

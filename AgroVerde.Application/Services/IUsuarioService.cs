using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IUsuarioService
{
    Task<AppResponse<UsuarioResponse>> RegistrarAsync(RegistrarUsuarioRequest request);

    Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest request);
}

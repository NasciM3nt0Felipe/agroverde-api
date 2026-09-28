using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IPropriedadeService
{
    Task<AppResponse<PropriedadeResponse>> CriarAsync(
        PropriedadeRequest request,
        int usuarioId);

    Task<AppResponse<List<PropriedadeResponse>>> ListarPorUsuarioIdAsync(
        int usuarioId);

    Task<AppResponse<PropriedadeResponse>> BuscarPorIdAsync(
        int id,
        int usuarioId);

    Task<AppResponse<PropriedadeResponse>> AtualizarAsync(
        int id,
        PropriedadeRequest request,
        int usuarioId);

    Task<AppResponse<PropriedadeResponse>> ExcluirAsync(
        int id,
        int usuarioId);
}

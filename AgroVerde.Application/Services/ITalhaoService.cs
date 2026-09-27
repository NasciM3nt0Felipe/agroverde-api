using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface ITalhaoService
{
    Task<AppResponse<List<TalhaoResponse>>> ListarPorPropriedadeIdAsync(
        int propriedadeId
    );

    Task<AppResponse<TalhaoResponse>> BuscarPorIdAsync(int id);

    Task<AppResponse<TalhaoResponse>> CriarAsync(TalhaoRequest request);

    Task<AppResponse<TalhaoResponse>> AtualizarAsync(
        int id,
        TalhaoRequest request
    );

    Task<AppResponse<TalhaoResponse>> ExcluirAsync(int id);
}
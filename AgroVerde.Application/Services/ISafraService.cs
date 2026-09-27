using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface ISafraService
{
    Task<AppResponse<SafraResponse>> CriarAsync(SafraRequest request);

    Task<AppResponse<SafraResponse>> BuscarPorIdAsync(int id);

    Task<AppResponse<List<SafraResponse>>> ListarPorTalhaoIdAsync(int talhaoId);

    Task<AppResponse<List<SafraResponse>>> ListarDisponiveisParaColheitaPorPropriedadeAsync(
    int propriedadeId);

    Task<AppResponse<SafraResponse>> AtualizarAsync(int id, SafraRequest request);

    Task<AppResponse<SafraResponse>> ExcluirAsync(int id);

}
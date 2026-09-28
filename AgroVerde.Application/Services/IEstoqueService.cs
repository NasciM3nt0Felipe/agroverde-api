using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IEstoqueService
{
    Task<AppResponse<EstoqueItemResponse>> CriarAsync(
        EstoqueItemRequest request);

    Task<AppResponse<EstoqueItemResponse>> BuscarPorIdAsync(
        int id);

    Task<AppResponse<List<EstoqueItemResponse>>> ListarPorPropriedadeIdAsync(
        int propriedadeId);

    Task<AppResponse<List<EstoqueItemResponse>>> ListarVacinasDisponiveisAsync(
        int propriedadeId);

    Task<AppResponse<EstoqueItemResponse>> AtualizarAsync(
        int id,
        EstoqueItemRequest request);

    Task<AppResponse<EstoqueItemResponse>> ExcluirAsync(
        int id);

    Task<AppResponse<EstoqueItemResponse>> ConsumirEstoqueAsync(
        int estoqueItemId,
        double quantidade);

    Task<AppResponse<EstoqueItemResponse>> RegistrarConsumoSafraAsync(
        ConsumoEstoqueRequest request);
}
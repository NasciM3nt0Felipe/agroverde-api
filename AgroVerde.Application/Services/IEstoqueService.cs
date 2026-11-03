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

    // Propriedade à qual a safra pertence (via talhão), ou null.
    Task<int?> ObterPropriedadeDaSafraAsync(int safraId);

    // Consumos da safra com nome, categoria e unidade do item,
    // do mais recente para o mais antigo. Categoria é opcional.
    Task<AppResponse<List<ConsumoSafraResponse>>> ListarConsumosPorSafraAsync(
        int safraId,
        string? categoria);
}
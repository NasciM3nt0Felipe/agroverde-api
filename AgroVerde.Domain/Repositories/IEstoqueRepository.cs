using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface IEstoqueRepository : IRepository<EstoqueItem>
{
    Task<List<EstoqueItem>> ListarPorPropriedadeIdAsync(int propriedadeId);

    Task<List<EstoqueItem>> ListarVacinasDisponiveisPorPropriedadeAsync(
        int propriedadeId);

    Task<EstoqueItem?> BuscarPorNomeCategoriaEPropriedadeAsync(
        int propriedadeId,
        string nome,
        string categoria);

    Task<EstoqueInsumo> InserirConsumoInsumoAsync(EstoqueInsumo consumo);

    Task<bool> ExisteConsumoPorSafraAsync(int safraId);

    // Consumos da safra, do mais recente para o mais antigo.
    Task<List<EstoqueInsumo>> ListarConsumosPorSafraIdAsync(int safraId);

    Task<bool> ExisteConsumoPorSafraECategoriaAsync(
        int safraId,
        string categoria);

    Task<EstoqueInsumo?> BuscarUltimoConsumoPorCategoriaAsync(
        int safraId,
        string categoria);

    Task RegistrarConsumoSafraAsync(
EstoqueItem item,
EstoqueInsumo consumo);

// Grava o item de estoque e sua despesa financeira
// na mesma transação.
Task<EstoqueItem> InserirComLancamentoAsync(
    EstoqueItem item,
    TransacaoFinanceira? lancamento);
}
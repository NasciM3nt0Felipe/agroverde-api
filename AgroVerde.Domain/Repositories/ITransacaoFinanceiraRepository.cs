using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface ITransacaoFinanceiraRepository : IRepository<TransacaoFinanceira>
{
    Task<IEnumerable<TransacaoFinanceira>> BuscarPorPeriodoAsync(
        DateTime inicio,
        DateTime fim);

    Task<IEnumerable<TransacaoFinanceira>> ListarPorPropriedadeIdAsync(
        int propriedadeId);
}
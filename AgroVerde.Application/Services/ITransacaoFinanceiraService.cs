using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface ITransacaoFinanceiraService
{
    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>> ListarTodosAsync();
    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>> BuscarPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<AppResponse<TransacaoFinanceiraResponse>> BuscarPorIdAsync(int id);
    Task<AppResponse<TransacaoFinanceiraResponse>> InserirAsync(TransacaoFinanceiraRequest request);
    Task<AppResponse<bool>> AtualizarAsync(int id, TransacaoFinanceiraRequest request);
    Task<AppResponse<bool>> ExcluirAsync(int id);
    Task<AppResponse<ResumoFinanceiroResponse>> ObterResumoFinanceiroAsync();
}

using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface ITransacaoFinanceiraService
{
    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        ListarTodosAsync(int usuarioId);

    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        BuscarPorPeriodoAsync(DateTime inicio, DateTime fim, int usuarioId);

    Task<AppResponse<TransacaoFinanceiraResponse>>
        BuscarPorIdAsync(int id, int usuarioId);

    Task<AppResponse<TransacaoFinanceiraResponse>>
        InserirAsync(TransacaoFinanceiraRequest request, int usuarioId);

    Task<AppResponse<bool>>
        AtualizarAsync(int id, TransacaoFinanceiraRequest request, int usuarioId);

    Task<AppResponse<bool>>
        ExcluirAsync(int id, int usuarioId);

    Task<AppResponse<ResumoFinanceiroResponse>>
        ObterResumoFinanceiroAsync(int usuarioId);
}

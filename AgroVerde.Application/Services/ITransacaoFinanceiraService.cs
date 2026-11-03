
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface ITransacaoFinanceiraService
{
    // Mensagem usada para "não existe ou é de outro usuário"
    // (o controller a usa para responder 404).
    public const string MensagemNaoEncontrada = "Transação não encontrada.";

    // Mensagem usada quando a propriedade filtrada não pertence ao usuário.
    public const string MensagemPropriedadeNaoEncontrada =
        "Propriedade não encontrada.";

    // propriedadeId opcional: quando informado, restringe a consulta
    // à propriedade (que precisa pertencer ao usuário).
    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        ListarTodosAsync(int usuarioId, int? propriedadeId = null);

    Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        BuscarPorPeriodoAsync(
            DateTime inicio,
            DateTime fim,
            int usuarioId,
            int? propriedadeId = null);

    Task<AppResponse<TransacaoFinanceiraResponse>>
        BuscarPorIdAsync(int id, int usuarioId);

    Task<AppResponse<TransacaoFinanceiraResponse>>
        InserirAsync(TransacaoFinanceiraRequest request, int usuarioId);

    Task<AppResponse<bool>>
        AtualizarAsync(int id, TransacaoFinanceiraRequest request, int usuarioId);

    Task<AppResponse<bool>>
        ExcluirAsync(int id, int usuarioId);

    Task<AppResponse<ResumoFinanceiroResponse>>
        ObterResumoFinanceiroAsync(int usuarioId, int? propriedadeId = null);
}

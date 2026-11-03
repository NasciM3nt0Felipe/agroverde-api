using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IPropriedadeService
{
    // Mensagem usada para "não existe ou é de outro usuário" (o controller a usa para responder 404).
    public const string MensagemNaoEncontrada = "Propriedade não encontrada.";

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

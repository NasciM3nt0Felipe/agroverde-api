using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IFuncionarioService
{
    // Mensagens que o controller transforma em HTTP 404.
    public const string MensagemNaoEncontrado = "Funcionário não encontrado.";
    public const string MensagemPropriedadeNaoEncontrada = "Propriedade não encontrada.";

    // propriedadeId nulo = todas as propriedades do usuário.
    Task<AppResponse<List<FuncionarioResponse>>> ListarAsync(
        int? propriedadeId,
        bool somenteAtivos,
        int usuarioId);

    Task<AppResponse<FuncionarioResponse>> BuscarPorIdAsync(int id, int usuarioId);

    Task<AppResponse<FuncionarioResponse>> CriarAsync(
        FuncionarioRequest request,
        int usuarioId);

    Task<AppResponse<FuncionarioResponse>> AtualizarAsync(
        int id,
        FuncionarioRequest request,
        int usuarioId);

    Task<AppResponse<FuncionarioResponse>> DesligarAsync(
        int id,
        DesligarFuncionarioRequest request,
        int usuarioId);

    Task<AppResponse<bool>> ExcluirAsync(int id, int usuarioId);
}

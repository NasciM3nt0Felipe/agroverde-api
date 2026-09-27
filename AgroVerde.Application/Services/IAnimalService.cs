using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IAnimalService
{
    Task<AppResponse<IEnumerable<AnimalResponse>>> ListarTodosAsync();
    Task<AppResponse<AnimalDetalhesResponse>> BuscarPorIdAsync(int id);
    Task<AppResponse<AnimalResponse>> InserirAsync(AnimalRequest request);
    Task<AppResponse<bool>> AtualizarAsync(int id, AnimalRequest request);
    Task<AppResponse<bool>> ExcluirAsync(int id);

    Task<AppResponse<PesagemResponse>> AdicionarPesagemAsync(PesagemRequest request);
    Task<AppResponse<VacinacaoResponse>> AdicionarVacinacaoAsync(VacinacaoRequest request);
    Task<AppResponse<ControleSanitarioResponse>> AdicionarControleSanitarioAsync(ControleSanitarioRequest request);
    Task<AppResponse<RegistroReproducaoResponse>> AdicionarRegistroReproducaoAsync(RegistroReproducaoRequest request);
}
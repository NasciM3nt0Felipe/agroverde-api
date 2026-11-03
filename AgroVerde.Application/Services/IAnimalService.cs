using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public interface IAnimalService
{
    Task<AppResponse<IEnumerable<AnimalResponse>>> ListarTodosAsync();
    Task<AppResponse<AnimalDetalhesResponse>> BuscarPorIdAsync(int id);
    Task<AppResponse<AnimalResponse>> InserirAsync(AnimalRequest request);
    Task<AppResponse<AnimalResponse>> AtualizarAsync(int id, AnimalRequest request);
    Task<AppResponse<bool>> ExcluirAsync(int id);

    Task<AppResponse<PesagemResponse>> AdicionarPesagemAsync(PesagemRequest request);
    Task<AppResponse<VacinacaoResponse>> AdicionarVacinacaoAsync(VacinacaoRequest request);
    Task<AppResponse<ControleSanitarioResponse>> AdicionarControleSanitarioAsync(ControleSanitarioRequest request);
    Task<AppResponse<RegistroReproducaoResponse>> AdicionarRegistroReproducaoAsync(RegistroReproducaoRequest request);

    Task<AppResponse<IEnumerable<AnimalResponse>>> ListarPorPropriedadeAsync(int propriedadeId);
    Task<int?> ObterPropriedadeDoAnimalAsync(int animalId);
    Task<int?> ObterPropriedadeDoRegistroAsync(TipoRegistroAnimal tipo, int id);
    Task<AppResponse<bool>> ExcluirRegistroAsync(TipoRegistroAnimal tipo, int id);

    Task<AppResponse<List<PesagemResponse>>> ListarPesagensPorPropriedadeAsync(int propriedadeId);
    Task<AppResponse<List<VacinacaoResponse>>> ListarVacinacoesPorPropriedadeAsync(int propriedadeId);
    Task<AppResponse<List<ControleSanitarioResponse>>> ListarControlesSanitariosPorPropriedadeAsync(int propriedadeId);
    Task<AppResponse<List<RegistroReproducaoResponse>>> ListarReproducoesPorPropriedadeAsync(int propriedadeId);
}
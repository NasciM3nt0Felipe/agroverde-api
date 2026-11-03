using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.Services
{
    public interface IVeiculoService
    {
        Task<AppResponse<IEnumerable<VeiculoResponse>>> ObterTodosAsync();
        Task<AppResponse<VeiculoResponse>> ObterPorIdAsync(int id);
        Task<AppResponse<IEnumerable<VeiculoResponse>>> ObterPorStatusAsync(StatusVeiculo status);
        Task<AppResponse<VeiculoResponse>> CriarAsync(VeiculoRequest request);
        Task<AppResponse<VeiculoResponse>> AtualizarAsync(int id, VeiculoRequest request);
        Task<AppResponse<bool>> AtualizarHodometroAsync(int id, AtualizarHodometroRequest request);
        Task<AppResponse<bool>> RemoverAsync(int id);

        Task<AppResponse<List<VeiculoResponse>>> ObterPorPropriedadeAsync(int propriedadeId);

        // Propriedade dona do veículo / do registro (null se não existir).
        Task<int?> ObterPropriedadeDoVeiculoAsync(int veiculoId);
        Task<int?> ObterPropriedadeDoAbastecimentoAsync(int id);
        Task<int?> ObterPropriedadeDaManutencaoAsync(int id);

        Task<AppResponse<List<AbastecimentoResponse>>> ListarAbastecimentosAsync(int veiculoId);
        Task<AppResponse<AbastecimentoResponse>> CriarAbastecimentoAsync(AbastecimentoRequest request);
        Task<AppResponse<AbastecimentoResponse>> AtualizarAbastecimentoAsync(int id, AbastecimentoRequest request);
        Task<AppResponse<bool>> ExcluirAbastecimentoAsync(int id);

        Task<AppResponse<List<ManutencaoResponse>>> ListarManutencoesAsync(int veiculoId);
        Task<AppResponse<ManutencaoResponse>> CriarManutencaoAsync(ManutencaoRequest request);
        Task<AppResponse<ManutencaoResponse>> AtualizarManutencaoAsync(int id, ManutencaoRequest request);
        Task<AppResponse<bool>> ExcluirManutencaoAsync(int id);
    }
}

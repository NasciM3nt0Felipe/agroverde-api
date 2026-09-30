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
    }
}
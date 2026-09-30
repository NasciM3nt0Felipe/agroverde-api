using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.Services
{
    public interface IPessoaService
    {
        Task<AppResponse<IEnumerable<PessoaResponse>>> ObterTodosAsync();
        Task<AppResponse<PessoaResponse>> ObterPorIdAsync(int id);
        Task<AppResponse<IEnumerable<PessoaResponse>>> ObterPorTipoAsync(TipoPessoa tipo);
        Task<AppResponse<PessoaResponse>> CriarAsync(PessoaRequest request);
        Task<AppResponse<PessoaResponse>> AtualizarAsync(int id, PessoaRequest request);
        Task<AppResponse<bool>> AlterarStatusAsync(int id, bool ativo);
        Task<AppResponse<bool>> RemoverAsync(int id);
    }
}
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class TalhaoService : ITalhaoService
{
    private readonly ITalhaoRepository _talhaoRepository;

    public TalhaoService(ITalhaoRepository talhaoRepository)
    {
        _talhaoRepository = talhaoRepository;
    }

    public async Task<AppResponse<TalhaoResponse>> CriarAsync(TalhaoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Informe o nome do talhão."
                );
            }

            if (request.Area <= 0)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Informe uma área válida."
                );
            }

            var talhao = new Talhao
            {
                PropriedadeId = request.PropriedadeId,
                Nome = request.Nome.Trim(),
                Area = request.Area,
                TipoSolo = request.TipoSolo,
                Observacao = request.Observacao,
                Ativo = request.Ativo
            };

            var talhaoCriado = await _talhaoRepository.InserirAsync(talhao);

            var response = new TalhaoResponse
            {
                Id = talhaoCriado.Id,
                PropriedadeId = talhaoCriado.PropriedadeId,
                Nome = talhaoCriado.Nome,
                Area = talhaoCriado.Area,
                TipoSolo = talhaoCriado.TipoSolo,
                Observacao = talhaoCriado.Observacao,
                Ativo = talhaoCriado.Ativo
            };

            return AppResponse<TalhaoResponse>.Ok(
                "Talhão cadastrado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<TalhaoResponse>.Fail(
                "Erro ao cadastrar o talhão.",
                ex
            );
        }
    }

    public async Task<AppResponse<TalhaoResponse>> BuscarPorIdAsync(int id)
    {
        try
        {
            var talhao = await _talhaoRepository.BuscarPorIdAsync(id);

            if (talhao == null)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Talhão não encontrado."
                );
            }

            var response = new TalhaoResponse
            {
                Id = talhao.Id,
                PropriedadeId = talhao.PropriedadeId,
                Nome = talhao.Nome,
                Area = talhao.Area,
                TipoSolo = talhao.TipoSolo,
                Observacao = talhao.Observacao,
                Ativo = talhao.Ativo
            };

            return AppResponse<TalhaoResponse>.Ok(
                "Talhão encontrado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<TalhaoResponse>.Fail(
                "Erro ao buscar o talhão.",
                ex
            );
        }
    }

    public async Task<AppResponse<List<TalhaoResponse>>> ListarPorPropriedadeIdAsync(
        int propriedadeId)
    {
        try
        {
            var talhoes = await _talhaoRepository
                .ListarPorPropriedadeIdAsync(propriedadeId);

            var response = talhoes.Select(talhao => new TalhaoResponse
            {
                Id = talhao.Id,
                PropriedadeId = talhao.PropriedadeId,
                Nome = talhao.Nome,
                Area = talhao.Area,
                TipoSolo = talhao.TipoSolo,
                Observacao = talhao.Observacao,
                Ativo = talhao.Ativo
            }).ToList();

            return AppResponse<List<TalhaoResponse>>.Ok(
                "Talhões encontrados com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<List<TalhaoResponse>>.Fail(
                "Erro ao listar os talhões da propriedade.",
                ex
            );
        }
    }

    public async Task<AppResponse<TalhaoResponse>> AtualizarAsync(
        int id,
        TalhaoRequest request)
    {
        try
        {
            var talhao = await _talhaoRepository.BuscarPorIdAsync(id);

            if (talhao == null)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Talhão não encontrado."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Informe o nome do talhão."
                );
            }

            if (request.Area <= 0)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Informe uma área válida."
                );
            }

            talhao.PropriedadeId = request.PropriedadeId;
            talhao.Nome = request.Nome.Trim();
            talhao.Area = request.Area;
            talhao.TipoSolo = request.TipoSolo;
            talhao.Observacao = request.Observacao;
            talhao.Ativo = request.Ativo;

            await _talhaoRepository.AtualizarAsync(talhao);

            var response = new TalhaoResponse
            {
                Id = talhao.Id,
                PropriedadeId = talhao.PropriedadeId,
                Nome = talhao.Nome,
                Area = talhao.Area,
                TipoSolo = talhao.TipoSolo,
                Observacao = talhao.Observacao,
                Ativo = talhao.Ativo
            };

            return AppResponse<TalhaoResponse>.Ok(
                "Talhão atualizado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<TalhaoResponse>.Fail(
                "Erro ao atualizar o talhão.",
                ex
            );
        }
    }

    public async Task<AppResponse<TalhaoResponse>> ExcluirAsync(int id)
    {
        try
        {
            var talhao = await _talhaoRepository.BuscarPorIdAsync(id);

            if (talhao == null)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    "Talhão não encontrado."
                );
            }

            await _talhaoRepository.ExcluirAsync(id);

            var response = new TalhaoResponse
            {
                Id = talhao.Id,
                PropriedadeId = talhao.PropriedadeId,
                Nome = talhao.Nome,
                Area = talhao.Area,
                TipoSolo = talhao.TipoSolo,
                Observacao = talhao.Observacao,
                Ativo = talhao.Ativo
            };

            return AppResponse<TalhaoResponse>.Ok(
                "Talhão excluído com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<TalhaoResponse>.Fail(
                "Erro ao excluir o talhão.",
                ex
            );
        }
    }
}
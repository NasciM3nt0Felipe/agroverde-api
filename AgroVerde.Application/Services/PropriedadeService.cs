using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class PropriedadeService : IPropriedadeService
{
    private readonly IPropriedadeRepository _propriedadeRepository;

    public PropriedadeService(IPropriedadeRepository propriedadeRepository)
    {
        _propriedadeRepository = propriedadeRepository;
    }

    public async Task<AppResponse<PropriedadeResponse>> CriarAsync(
        PropriedadeRequest request,
        int usuarioId)
    {
        try
        {
            var erro = Validar(request);

            if (erro != null)
            {
                return AppResponse<PropriedadeResponse>.Fail(erro);
            }

            var propriedade = new Propriedade
            {
                UsuarioId = usuarioId,
                Nome = request.Nome.Trim(),
                Localizacao = request.Localizacao.Trim(),
                AreaTotal = request.AreaTotal
            };

            var propriedadeCriada = await _propriedadeRepository.InserirAsync(propriedade);

            return AppResponse<PropriedadeResponse>.Ok(
                "Propriedade cadastrada com sucesso.",
                ParaResponse(propriedadeCriada)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<PropriedadeResponse>.Fail(
                "Erro ao cadastrar a propriedade.",
                ex
            );
        }
    }

    public async Task<AppResponse<List<PropriedadeResponse>>> ListarPorUsuarioIdAsync(
        int usuarioId)
    {
        try
        {
            var propriedades = await _propriedadeRepository
                .ListarPorUsuarioIdAsync(usuarioId);

            var response = propriedades.Select(ParaResponse).ToList();

            return AppResponse<List<PropriedadeResponse>>.Ok(
                "Propriedades encontradas com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<List<PropriedadeResponse>>.Fail(
                "Erro ao listar as propriedades.",
                ex
            );
        }
    }

    public async Task<AppResponse<PropriedadeResponse>> BuscarPorIdAsync(
        int id,
        int usuarioId)
    {
        try
        {
            var propriedade = await _propriedadeRepository.BuscarPorIdAsync(id);

            // Propriedade de outro usuário é tratada como "não encontrada".
            if (propriedade == null || propriedade.UsuarioId != usuarioId)
            {
                return AppResponse<PropriedadeResponse>.Fail(
                    "Propriedade não encontrada."
                );
            }

            return AppResponse<PropriedadeResponse>.Ok(
                "Propriedade encontrada com sucesso.",
                ParaResponse(propriedade)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<PropriedadeResponse>.Fail(
                "Erro ao buscar a propriedade.",
                ex
            );
        }
    }

    public async Task<AppResponse<PropriedadeResponse>> AtualizarAsync(
        int id,
        PropriedadeRequest request,
        int usuarioId)
    {
        try
        {
            var propriedade = await _propriedadeRepository.BuscarPorIdAsync(id);

            if (propriedade == null || propriedade.UsuarioId != usuarioId)
            {
                return AppResponse<PropriedadeResponse>.Fail(
                    "Propriedade não encontrada."
                );
            }

            var erro = Validar(request);

            if (erro != null)
            {
                return AppResponse<PropriedadeResponse>.Fail(erro);
            }

            // Atenção (módulo de Talhão): reduzir a AreaTotal aqui pode fazer a soma
            // dos talhões passar de 90% da nova área. Essa checagem cruzada ainda
            // não existe, pois Propriedade não conhece Talhão.
            propriedade.Nome = request.Nome.Trim();
            propriedade.Localizacao = request.Localizacao.Trim();
            propriedade.AreaTotal = request.AreaTotal;

            await _propriedadeRepository.AtualizarAsync(propriedade);

            return AppResponse<PropriedadeResponse>.Ok(
                "Propriedade atualizada com sucesso.",
                ParaResponse(propriedade)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<PropriedadeResponse>.Fail(
                "Erro ao atualizar a propriedade.",
                ex
            );
        }
    }

    public async Task<AppResponse<PropriedadeResponse>> ExcluirAsync(
        int id,
        int usuarioId)
    {
        try
        {
            var propriedade = await _propriedadeRepository.BuscarPorIdAsync(id);

            if (propriedade == null || propriedade.UsuarioId != usuarioId)
            {
                return AppResponse<PropriedadeResponse>.Fail(
                    "Propriedade não encontrada."
                );
            }

            await _propriedadeRepository.ExcluirAsync(id);

            return AppResponse<PropriedadeResponse>.Ok(
                "Propriedade excluída com sucesso.",
                ParaResponse(propriedade)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<PropriedadeResponse>.Fail(
                "Erro ao excluir a propriedade.",
                ex
            );
        }
    }

    private static string? Validar(PropriedadeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return "Informe o nome da propriedade.";
        }

        if (string.IsNullOrWhiteSpace(request.Localizacao))
        {
            return "Informe a localização da propriedade.";
        }

        if (request.AreaTotal <= 0)
        {
            return "Informe uma área total válida.";
        }

        return null;
    }

    private static PropriedadeResponse ParaResponse(Propriedade propriedade)
    {
        return new PropriedadeResponse
        {
            Id = propriedade.Id,
            UsuarioId = propriedade.UsuarioId,
            Nome = propriedade.Nome,
            Localizacao = propriedade.Localizacao,
            AreaTotal = propriedade.AreaTotal,
            DataCriacao = propriedade.DataCriacao
        };
    }
}

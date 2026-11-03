using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class TalhaoService : ITalhaoService
{
    // Os talhões somados não podem passar de 90% da área total (10% de reserva técnica).
    private const double PercentualAreaCultivavel = 0.90;

    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly ISafraRepository _safraRepository;

    public TalhaoService(
        ITalhaoRepository talhaoRepository,
        IPropriedadeRepository propriedadeRepository,
        ISafraRepository safraRepository)
    {
        _talhaoRepository = talhaoRepository;
        _propriedadeRepository = propriedadeRepository;
        _safraRepository = safraRepository;
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

            var erroArea = await ValidarAreaAsync(
                request.PropriedadeId, request.Area, null);

            if (erroArea != null)
            {
                return AppResponse<TalhaoResponse>.Fail(erroArea);
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

            // Na edição, a área anterior do próprio talhão não entra na soma.
            var erroArea = await ValidarAreaAsync(
                talhao.PropriedadeId, request.Area, talhao.Id);

            if (erroArea != null)
            {
                return AppResponse<TalhaoResponse>.Fail(erroArea);
            }

            // A propriedade do talhão não é alterada.
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

            var safras = await _safraRepository.ListarPorTalhaoIdAsync(id);

            if (safras.Count > 0)
            {
                return AppResponse<TalhaoResponse>.Fail(
                    $"Não é possível excluir o talhão \"{talhao.Nome}\" porque ele possui "
                    + $"{safras.Count} safra(s) vinculada(s). Exclua as safras antes."
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

    // Soma dos demais talhões da propriedade + área informada <= 90% da área total.
    private async Task<string?> ValidarAreaAsync(
        int propriedadeId,
        double area,
        int? ignorarTalhaoId)
    {
        var propriedade = await _propriedadeRepository.BuscarPorIdAsync(propriedadeId);

        if (propriedade == null)
        {
            return "Propriedade não encontrada.";
        }

        var talhoes = await _talhaoRepository.ListarPorPropriedadeIdAsync(propriedadeId);

        var areaJaUtilizada = talhoes
            .Where(t => t.Id != ignorarTalhaoId)
            .Sum(t => t.Area);

        var areaMaxima = propriedade.AreaTotal * PercentualAreaCultivavel;

        if (areaJaUtilizada + area > areaMaxima + 1e-9)
        {
            var areaDisponivel = Math.Max(0, areaMaxima - areaJaUtilizada);

            return "Área excedida. A propriedade mantém 10% como reserva técnica. "
                + $"Área máxima para talhões: {FormatarArea(areaMaxima)} ha. "
                + $"Disponível para novos talhões: {FormatarArea(areaDisponivel)} ha.";
        }

        return null;
    }

    private static string FormatarArea(double valor)
    {
        if (valor % 1 == 0)
        {
            return valor.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        }

        return valor
            .ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            .Replace('.', ',');
    }
}
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class PropriedadeService : IPropriedadeService
{
    // Os talhões somados não podem passar de 90% da área total (10% de reserva técnica).
    private const double PercentualAreaCultivavel = 0.90;

    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ITransacaoFinanceiraRepository _financeiroRepository;

    public PropriedadeService(
        IPropriedadeRepository propriedadeRepository,
        ITalhaoRepository talhaoRepository,
        IEstoqueRepository estoqueRepository,
        IAnimalRepository animalRepository,
        ITransacaoFinanceiraRepository financeiroRepository)
    {
        _propriedadeRepository = propriedadeRepository;
        _talhaoRepository = talhaoRepository;
        _estoqueRepository = estoqueRepository;
        _animalRepository = animalRepository;
        _financeiroRepository = financeiroRepository;
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
                Localizacao = MontarLocalizacao(request),
                AreaTotal = request.AreaTotal,
                Cidade = LimparOpcional(request.Cidade),
                Estado = LimparOpcional(request.Estado),
                Descricao = LimparOpcional(request.Descricao)
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
                    IPropriedadeService.MensagemNaoEncontrada
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
                    IPropriedadeService.MensagemNaoEncontrada
                );
            }

            var erro = Validar(request);

            if (erro != null)
            {
                return AppResponse<PropriedadeResponse>.Fail(erro);
            }

            // Não permite reduzir a área abaixo do que os talhões já cadastrados exigem.
            var talhoes = await _talhaoRepository.ListarPorPropriedadeIdAsync(id);
            var areaTalhoes = talhoes.Sum(t => t.Area);
            var areaMinima = areaTalhoes / PercentualAreaCultivavel;

            if (areaTalhoes > 0 && request.AreaTotal < areaMinima - 1e-9)
            {
                return AppResponse<PropriedadeResponse>.Fail(
                    "Área insuficiente. Os talhões cadastrados somam "
                    + $"{FormatarArea(areaTalhoes)} ha e a propriedade mantém 10% como reserva técnica. "
                    + $"Área total mínima: {FormatarArea(Math.Ceiling(areaMinima * 100) / 100)} ha."
                );
            }

            propriedade.Nome = request.Nome.Trim();
            propriedade.Localizacao = MontarLocalizacao(request);
            propriedade.AreaTotal = request.AreaTotal;
            propriedade.Cidade = LimparOpcional(request.Cidade);
            propriedade.Estado = LimparOpcional(request.Estado);
            propriedade.Descricao = LimparOpcional(request.Descricao);

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
                    IPropriedadeService.MensagemNaoEncontrada
                );
            }

            // Sem exclusão em cascata: se houver registros vinculados, bloqueia.
            // (Safras pertencem a talhões, então já ficam cobertas pelos talhões.)
            var vinculos = new List<string>();

            var qtdTalhoes = (await _talhaoRepository.ListarPorPropriedadeIdAsync(id)).Count;
            var qtdEstoque = (await _estoqueRepository.ListarPorPropriedadeIdAsync(id)).Count;
            var qtdAnimais = (await _animalRepository.ListarPorPropriedadeIdAsync(id)).Count();
            var qtdLancamentos = (await _financeiroRepository.ListarPorPropriedadeIdAsync(id)).Count();

            if (qtdTalhoes > 0) vinculos.Add($"{qtdTalhoes} talhão(ões)");
            if (qtdEstoque > 0) vinculos.Add($"{qtdEstoque} item(ns) de estoque");
            if (qtdAnimais > 0) vinculos.Add($"{qtdAnimais} animal(is)");
            if (qtdLancamentos > 0) vinculos.Add($"{qtdLancamentos} lançamento(s) financeiro(s)");

            if (vinculos.Count > 0)
            {
                return AppResponse<PropriedadeResponse>.Fail(
                    $"Não é possível excluir a propriedade \"{propriedade.Nome}\" porque ela possui "
                    + $"registros vinculados: {string.Join(", ", vinculos)}. "
                    + "Remova esses registros antes de excluí-la."
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

        if (request.AreaTotal <= 0 || !double.IsFinite(request.AreaTotal))
        {
            return "Informe uma área total válida.";
        }

        return null;
    }

    // Localizacao é obrigatória no banco, mas o app envia cidade/estado. Se Localizacao
    // não vier, monta "Cidade - Estado"; se nada vier, usa "Não informada".
    private static string MontarLocalizacao(PropriedadeRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Localizacao))
        {
            return request.Localizacao.Trim();
        }

        var partes = new[] { request.Cidade, request.Estado }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());

        var montada = string.Join(" - ", partes);

        return string.IsNullOrWhiteSpace(montada) ? "Não informada" : montada;
    }

    private static string? LimparOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

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

    private static PropriedadeResponse ParaResponse(Propriedade propriedade)
    {
        return new PropriedadeResponse
        {
            Id = propriedade.Id,
            UsuarioId = propriedade.UsuarioId,
            Nome = propriedade.Nome,
            Localizacao = propriedade.Localizacao,
            AreaTotal = propriedade.AreaTotal,
            Cidade = propriedade.Cidade,
            Estado = propriedade.Estado,
            Descricao = propriedade.Descricao,
            DataCriacao = propriedade.DataCriacao
        };
    }
}

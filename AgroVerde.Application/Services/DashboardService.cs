using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly ISafraRepository _safraRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ITransacaoFinanceiraRepository _financeiroRepository;

    public DashboardService(
        ITalhaoRepository talhaoRepository,
        ISafraRepository safraRepository,
        IAnimalRepository animalRepository,
        ITransacaoFinanceiraRepository financeiroRepository)
    {
        _talhaoRepository = talhaoRepository;
        _safraRepository = safraRepository;
        _animalRepository = animalRepository;
        _financeiroRepository = financeiroRepository;
    }

    public async Task<AppResponse<DashboardResponse>> ObterDashboardAsync(
        int propriedadeId)
    {
        try
        {
            if (propriedadeId <= 0)
            {
                return AppResponse<DashboardResponse>.Fail(
                    "Propriedade inválida.",
                    new List<string> { "Informe uma propriedade válida." });
            }

            var talhoes =
                await _talhaoRepository.ListarPorPropriedadeIdAsync(propriedadeId);

            var totalSafras = 0;

            foreach (var talhao in talhoes)
            {
                var safras =
                    await _safraRepository.ListarPorTalhaoIdAsync(talhao.Id);

                totalSafras += safras.Count(s =>
                    s.Status.Equals(
                        "Planejada",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    s.Status.Equals(
                        "Em andamento",
                        StringComparison.OrdinalIgnoreCase));
            }

            var animais =
                await _animalRepository.ListarPorPropriedadeIdAsync(propriedadeId);

            var transacoes =
                await _financeiroRepository.ListarPorPropriedadeIdAsync(
                    propriedadeId);

            var receitas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);

            var despesas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);

            var saldo = receitas - despesas;

            var response = new DashboardResponse(
                talhoes.Count,
                totalSafras,
                animais.Count(),
                receitas,
                despesas,
                saldo);

            return AppResponse<DashboardResponse>.Ok(
                "Dashboard carregado com sucesso.",
                response);
        }
        catch (Exception ex)
        {
            return AppResponse<DashboardResponse>.Fail(
                "Erro ao carregar o dashboard.",
                ex);
        }
    }
}
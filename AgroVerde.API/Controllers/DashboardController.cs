using System.Security.Claims;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AgroVerde.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IPropriedadeService _propriedadeService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardController> _logger;

    private static readonly TimeSpan TempoCache = TimeSpan.FromMinutes(2);

    // Chave única do cache do dashboard, usada também pelo Financeiro
    // para descartar o cache após inclusões, edições e exclusões.
    public static string ChaveCache(int propriedadeId) =>
        $"dashboard_propriedade_{propriedadeId}";

    public DashboardController(
        IDashboardService dashboardService,
        IPropriedadeService propriedadeService,
        IMemoryCache cache,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _propriedadeService = propriedadeService;
        _cache = cache;
        _logger = logger;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{propriedadeId:int}")]
    public async Task<IActionResult> Obter(int propriedadeId)
    {
        if (propriedadeId <= 0)
        {
            return BadRequest(
                AppResponse<DashboardResponse>.Fail(
                    "Propriedade inválida.",
                    new List<string>
                    {
                        "Informe uma propriedade válida."
                    }));
        }

        // A propriedade precisa pertencer ao usuário do token.
        // A checagem vem antes do cache para nunca servir dados de terceiros.
        var propriedade = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, UsuarioId);

        if (!propriedade.Success || propriedade.Data is null)
        {
            return NotFound(
                AppResponse<DashboardResponse>.Fail(
                    IPropriedadeService.MensagemNaoEncontrada));
        }

        var cacheKey = ChaveCache(propriedadeId);

        if (_cache.TryGetValue(
            cacheKey,
            out AppResponse<DashboardResponse>? resultadoCache))
        {
            _logger.LogInformation(
                "Cache HIT para o dashboard da propriedade {PropriedadeId}.",
                propriedadeId);

            return Ok(resultadoCache);
        }

        _logger.LogInformation(
            "Cache MISS para o dashboard da propriedade {PropriedadeId}.",
            propriedadeId);

        var resultado =
            await _dashboardService.ObterDashboardAsync(propriedadeId);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        _cache.Set(
            cacheKey,
            resultado,
            TempoCache);

        return Ok(resultado);
    }
}
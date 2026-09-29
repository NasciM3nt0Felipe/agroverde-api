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
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardController> _logger;

    private static readonly TimeSpan TempoCache = TimeSpan.FromMinutes(2);

    public DashboardController(
        IDashboardService dashboardService,
        IMemoryCache cache,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _cache = cache;
        _logger = logger;
    }

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

        var cacheKey = $"dashboard_propriedade_{propriedadeId}";

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
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
public class PropriedadeController : ControllerBase
{
    private static readonly TimeSpan TempoCache = TimeSpan.FromMinutes(2);

    private readonly IPropriedadeService _propriedadeService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PropriedadeController> _logger;

    public PropriedadeController(
        IPropriedadeService propriedadeService,
        IMemoryCache cache,
        ILogger<PropriedadeController> logger)
    {
        _propriedadeService = propriedadeService;
        _cache = cache;
        _logger = logger;
    }

    // O usuário vem SEMPRE do token JWT, nunca do corpo/query da requisição.
    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CacheKey => $"propriedades_usuario_{UsuarioId}";

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (_cache.TryGetValue(CacheKey, out AppResponse<List<PropriedadeResponse>>? emCache)
            && emCache != null)
        {
            _logger.LogInformation("CACHE HIT: {Chave}", CacheKey);
            return Ok(emCache);
        }

        _logger.LogInformation("CACHE MISS (buscando no banco): {Chave}", CacheKey);

        var resultado = await _propriedadeService.ListarPorUsuarioIdAsync(UsuarioId);

        if (resultado.Success)
        {
            _cache.Set(CacheKey, resultado, TempoCache);
        }

        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var resultado = await _propriedadeService.BuscarPorIdAsync(id, UsuarioId);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PropriedadeRequest request)
    {
        var resultado = await _propriedadeService.CriarAsync(request, UsuarioId);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        InvalidarCache();

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = resultado.Data!.Id },
            resultado
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] PropriedadeRequest request)
    {
        var resultado = await _propriedadeService.AtualizarAsync(id, request, UsuarioId);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        InvalidarCache();

        return Ok(resultado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _propriedadeService.ExcluirAsync(id, UsuarioId);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        InvalidarCache();

        return Ok(resultado);
    }

    private void InvalidarCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("CACHE INVALIDADO: {Chave}", CacheKey);
    }
}

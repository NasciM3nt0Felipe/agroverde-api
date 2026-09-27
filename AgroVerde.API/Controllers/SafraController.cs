using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SafraController : ControllerBase
{
    private readonly ISafraService _safraService;

    public SafraController(ISafraService safraService)
    {
        _safraService = safraService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(SafraRequest request)
    {
        var resultado = await _safraService.CriarAsync(request);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        return StatusCode(201, resultado);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var resultado = await _safraService.BuscarPorIdAsync(id);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }
    [HttpGet("talhao/{talhaoId}")]
    public async Task<IActionResult> ListarPorTalhaoId(int talhaoId)
    {
        var resultado = await _safraService.ListarPorTalhaoIdAsync(talhaoId);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, SafraRequest request)
    {
        var resultado = await _safraService.AtualizarAsync(id, request);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _safraService.ExcluirAsync(id);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }
    [HttpGet("disponiveis-colheita/propriedade/{propriedadeId}")]
    public async Task<IActionResult> ListarDisponiveisParaColheitaPorPropriedade(
    int propriedadeId)
    {
        var resultado = await _safraService
            .ListarDisponiveisParaColheitaPorPropriedadeAsync(propriedadeId);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        return Ok(resultado);
    }
}
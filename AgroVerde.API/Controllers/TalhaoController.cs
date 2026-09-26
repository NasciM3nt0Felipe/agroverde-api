using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TalhaoController : ControllerBase
{
    private readonly ITalhaoService _talhaoService;

    public TalhaoController(ITalhaoService talhaoService)
    {
        _talhaoService = talhaoService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var resultado = await _talhaoService.BuscarPorIdAsync(id);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TalhaoRequest request)
    {
        var resultado = await _talhaoService.CriarAsync(request);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = resultado.Data!.Id },
            resultado
        );
    }

    [HttpGet("propriedade/{propriedadeId}")]
    public async Task<IActionResult> ListarPorPropriedade(int propriedadeId)
    {
        var resultado = await _talhaoService
            .ListarPorPropriedadeIdAsync(propriedadeId);

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] TalhaoRequest request)
    {
        var resultado = await _talhaoService.AtualizarAsync(id, request);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _talhaoService.ExcluirAsync(id);

        if (!resultado.Success)
        {
            return NotFound(resultado);
        }

        return Ok(resultado);
    }
}
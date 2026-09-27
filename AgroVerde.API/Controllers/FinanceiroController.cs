using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceiroController : ControllerBase
{
    private readonly ITransacaoFinanceiraService _service;

    public FinanceiroController(ITransacaoFinanceiraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        if (inicio.HasValue && fim.HasValue)
        {
            var filtrados = await _service.BuscarPorPeriodoAsync(inicio.Value, fim.Value);
            return Ok(filtrados);
        }

        var result = await _service.ListarTodosAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var result = await _service.BuscarPorIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        var result = await _service.ObterResumoFinanceiroAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir([FromBody] TransacaoFinanceiraRequest request)
    {
        var result = await _service.InserirAsync(request);
        return CreatedAtAction(nameof(BuscarPorId), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] TransacaoFinanceiraRequest request)
    {
        var result = await _service.AtualizarAsync(id, request);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var result = await _service.ExcluirAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
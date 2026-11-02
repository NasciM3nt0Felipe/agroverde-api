
using System.Security.Claims;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinanceiroController : ControllerBase
{
    private readonly ITransacaoFinanceiraService _service;

    public FinanceiroController(ITransacaoFinanceiraService service)
    {
        _service = service;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ListarTodos(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim)
    {
        if (inicio.HasValue != fim.HasValue)
            return BadRequest(
                "Informe as duas datas para filtrar o período.");

        if (inicio.HasValue && fim.HasValue)
        {
            var filtrados = await _service.BuscarPorPeriodoAsync(
                inicio.Value, fim.Value, UsuarioId);

            return filtrados.Success
                ? Ok(filtrados)
                : BadRequest(filtrados);
        }

        var resultado = await _service.ListarTodosAsync(UsuarioId);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var resultado = await _service.BuscarPorIdAsync(id, UsuarioId);

        return resultado.Success
            ? Ok(resultado)
            : NotFound(resultado);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        var resultado = await _service.ObterResumoFinanceiroAsync(UsuarioId);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir(
        [FromBody] TransacaoFinanceiraRequest request)
    {
        var resultado = await _service.InserirAsync(request, UsuarioId);

        if (!resultado.Success || resultado.Data is null)
            return BadRequest(resultado);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = resultado.Data.Id },
            resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] TransacaoFinanceiraRequest request)
    {
        var resultado = await _service.AtualizarAsync(
            id, request, UsuarioId);

        if (!resultado.Success)
        {
            if (resultado.Message == "Transação não encontrada.")
                return NotFound(resultado);

            return BadRequest(resultado);
        }

        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _service.ExcluirAsync(id, UsuarioId);

        return resultado.Success
            ? Ok(resultado)
            : NotFound(resultado);
    }
}

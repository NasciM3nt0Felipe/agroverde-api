
using System.Security.Claims;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AgroVerde.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinanceiroController : ControllerBase
{
    private readonly ITransacaoFinanceiraService _service;
    private readonly IMemoryCache _cache;

    public FinanceiroController(
        ITransacaoFinanceiraService service,
        IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    // Descarta o cache do dashboard para que os totais reflitam
    // imediatamente a alteração feita no Financeiro.
    private void InvalidarDashboard(int propriedadeId) =>
        _cache.Remove(DashboardController.ChaveCache(propriedadeId));

    // Propriedade do lançamento (já validando o dono), ou null.
    private async Task<int?> PropriedadeDoLancamentoAsync(int id)
    {
        var atual = await _service.BuscarPorIdAsync(id, UsuarioId);

        return atual.Success && atual.Data is not null
            ? atual.Data.PropriedadeId
            : null;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool PropriedadeNaoEncontrada(string? mensagem) =>
        mensagem == ITransacaoFinanceiraService
            .MensagemPropriedadeNaoEncontrada;

    // GET api/Financeiro?propriedadeId=1&inicio=2026-01-01&fim=2026-01-31
    // propriedadeId é opcional: sem ele, lista todas as propriedades do usuário.
    [HttpGet]
    public async Task<IActionResult> ListarTodos(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        [FromQuery] int? propriedadeId)
    {
        if (inicio.HasValue != fim.HasValue)
            return BadRequest(
                "Informe as duas datas para filtrar o período.");

        if (inicio.HasValue && fim.HasValue)
        {
            var filtrados = await _service.BuscarPorPeriodoAsync(
                inicio.Value, fim.Value, UsuarioId, propriedadeId);

            if (filtrados.Success)
                return Ok(filtrados);

            return PropriedadeNaoEncontrada(filtrados.Message)
                ? NotFound(filtrados)
                : BadRequest(filtrados);
        }

        var resultado = await _service.ListarTodosAsync(
            UsuarioId, propriedadeId);

        return resultado.Success
            ? Ok(resultado)
            : NotFound(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var resultado = await _service.BuscarPorIdAsync(id, UsuarioId);

        return resultado.Success
            ? Ok(resultado)
            : NotFound(resultado);
    }

    // GET api/Financeiro/resumo?propriedadeId=1
    // Considera apenas lançamentos pagos.
    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo(
        [FromQuery] int? propriedadeId)
    {
        var resultado = await _service.ObterResumoFinanceiroAsync(
            UsuarioId, propriedadeId);

        return resultado.Success
            ? Ok(resultado)
            : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir(
        [FromBody] TransacaoFinanceiraRequest request)
    {
        var resultado = await _service.InserirAsync(request, UsuarioId);

        if (!resultado.Success || resultado.Data is null)
            return BadRequest(resultado);

        InvalidarDashboard(resultado.Data.PropriedadeId);

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
        var propriedadeId = await PropriedadeDoLancamentoAsync(id);

        var resultado = await _service.AtualizarAsync(
            id, request, UsuarioId);

        if (!resultado.Success)
        {
            if (resultado.Message ==
                ITransacaoFinanceiraService.MensagemNaoEncontrada)
                return NotFound(resultado);

            return BadRequest(resultado);
        }

        if (propriedadeId.HasValue)
            InvalidarDashboard(propriedadeId.Value);

        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var propriedadeId = await PropriedadeDoLancamentoAsync(id);

        var resultado = await _service.ExcluirAsync(id, UsuarioId);

        if (resultado.Success)
        {
            if (propriedadeId.HasValue)
                InvalidarDashboard(propriedadeId.Value);

            return Ok(resultado);
        }

        return resultado.Message ==
            ITransacaoFinanceiraService.MensagemNaoEncontrada
            ? NotFound(resultado)
            : BadRequest(resultado);
    }
}

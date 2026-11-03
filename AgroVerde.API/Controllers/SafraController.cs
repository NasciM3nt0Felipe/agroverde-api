using System.Security.Claims;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SafraController : ControllerBase
{
    private const string MensagemNaoEncontrada = "Safra não encontrada.";
    private const string MensagemTalhaoNaoEncontrado = "Talhão não encontrado.";

    private readonly ISafraService _safraService;
    private readonly ITalhaoService _talhaoService;
    private readonly IPropriedadeService _propriedadeService;

    public SafraController(
        ISafraService safraService,
        ITalhaoService talhaoService,
        IPropriedadeService propriedadeService)
    {
        _safraService = safraService;
        _talhaoService = talhaoService;
        _propriedadeService = propriedadeService;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> PropriedadeAutorizadaAsync(int propriedadeId)
    {
        if (propriedadeId <= 0)
            return false;

        var resultado = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, UsuarioId);

        return resultado.Success && resultado.Data != null;
    }

    // Talhão existente e de uma propriedade do usuário; senão, null.
    private async Task<TalhaoResponse?> TalhaoAutorizadoAsync(int talhaoId)
    {
        if (talhaoId <= 0)
            return null;

        var resultado = await _talhaoService.BuscarPorIdAsync(talhaoId);

        if (!resultado.Success || resultado.Data == null)
            return null;

        return await PropriedadeAutorizadaAsync(resultado.Data.PropriedadeId)
            ? resultado.Data
            : null;
    }

    // Safra existente cujo talhão pertence a uma propriedade do usuário.
    private async Task<(SafraResponse Safra, TalhaoResponse Talhao)?> SafraAutorizadaAsync(int id)
    {
        var resultado = await _safraService.BuscarPorIdAsync(id);

        if (!resultado.Success || resultado.Data == null)
            return null;

        var talhao = await TalhaoAutorizadoAsync(resultado.Data.TalhaoId);

        return talhao == null ? null : (resultado.Data, talhao);
    }

    private static AppResponse<SafraResponse> Falha(string mensagem) =>
        AppResponse<SafraResponse>.Fail(mensagem, [mensagem]);

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SafraRequest request)
    {
        if (await TalhaoAutorizadoAsync(request.TalhaoId) == null)
            return NotFound(Falha(MensagemTalhaoNaoEncontrado));

        var resultado = await _safraService.CriarAsync(request);

        if (!resultado.Success)
            return BadRequest(resultado);

        return StatusCode(201, resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var autorizada = await SafraAutorizadaAsync(id);

        if (autorizada == null)
            return NotFound(Falha(MensagemNaoEncontrada));

        return Ok(AppResponse<SafraResponse>.Ok(
            "Safra encontrada com sucesso.", autorizada.Value.Safra));
    }

    [HttpGet("talhao/{talhaoId:int}")]
    public async Task<IActionResult> ListarPorTalhaoId(int talhaoId)
    {
        if (await TalhaoAutorizadoAsync(talhaoId) == null)
            return NotFound(Falha(MensagemTalhaoNaoEncontrado));

        var resultado = await _safraService.ListarPorTalhaoIdAsync(talhaoId);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] SafraRequest request)
    {
        var autorizada = await SafraAutorizadaAsync(id);

        if (autorizada == null)
            return NotFound(Falha(MensagemNaoEncontrada));

        var talhaoDestino = await TalhaoAutorizadoAsync(request.TalhaoId);

        if (talhaoDestino == null)
            return NotFound(Falha(MensagemTalhaoNaoEncontrado));

        if (talhaoDestino.PropriedadeId != autorizada.Value.Talhao.PropriedadeId)
            return BadRequest(Falha(
                "Não é permitido transferir a safra para outra propriedade."));

        var resultado = await _safraService.AtualizarAsync(id, request);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await SafraAutorizadaAsync(id) == null)
            return NotFound(Falha(MensagemNaoEncontrada));

        var resultado = await _safraService.ExcluirAsync(id);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpGet("disponiveis-colheita/propriedade/{propriedadeId:int}")]
    public async Task<IActionResult> ListarDisponiveisParaColheitaPorPropriedade(
        int propriedadeId)
    {
        if (!await PropriedadeAutorizadaAsync(propriedadeId))
            return NotFound(Falha(IPropriedadeService.MensagemNaoEncontrada));

        var resultado = await _safraService
            .ListarDisponiveisParaColheitaPorPropriedadeAsync(propriedadeId);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }
}
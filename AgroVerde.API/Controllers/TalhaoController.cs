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
public class TalhaoController : ControllerBase
{
    private const string MensagemNaoEncontrado = "Talhão não encontrado.";

    private readonly ITalhaoService _talhaoService;
    private readonly IPropriedadeService _propriedadeService;

    public TalhaoController(
        ITalhaoService talhaoService,
        IPropriedadeService propriedadeService)
    {
        _talhaoService = talhaoService;
        _propriedadeService = propriedadeService;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // A propriedade precisa pertencer ao usuário do token.
    private async Task<bool> PropriedadeAutorizadaAsync(int propriedadeId)
    {
        if (propriedadeId <= 0)
            return false;

        var resultado = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, UsuarioId);

        return resultado.Success && resultado.Data != null;
    }

    // Talhão existente e de uma propriedade do usuário; senão, null.
    private async Task<TalhaoResponse?> TalhaoAutorizadoAsync(int id)
    {
        var resultado = await _talhaoService.BuscarPorIdAsync(id);

        if (!resultado.Success || resultado.Data == null)
            return null;

        return await PropriedadeAutorizadaAsync(resultado.Data.PropriedadeId)
            ? resultado.Data
            : null;
    }

    private static AppResponse<TalhaoResponse> Falha(string mensagem) =>
        AppResponse<TalhaoResponse>.Fail(mensagem, [mensagem]);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var talhao = await TalhaoAutorizadoAsync(id);

        if (talhao == null)
            return NotFound(Falha(MensagemNaoEncontrado));

        return Ok(AppResponse<TalhaoResponse>.Ok(
            "Talhão encontrado com sucesso.", talhao));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TalhaoRequest request)
    {
        if (!await PropriedadeAutorizadaAsync(request.PropriedadeId))
            return NotFound(Falha(IPropriedadeService.MensagemNaoEncontrada));

        var resultado = await _talhaoService.CriarAsync(request);

        if (!resultado.Success || resultado.Data == null)
            return BadRequest(resultado);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = resultado.Data.Id },
            resultado
        );
    }

    [HttpGet("propriedade/{propriedadeId:int}")]
    public async Task<IActionResult> ListarPorPropriedade(int propriedadeId)
    {
        if (!await PropriedadeAutorizadaAsync(propriedadeId))
            return NotFound(Falha(IPropriedadeService.MensagemNaoEncontrada));

        var resultado = await _talhaoService
            .ListarPorPropriedadeIdAsync(propriedadeId);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] TalhaoRequest request)
    {
        var talhao = await TalhaoAutorizadoAsync(id);

        if (talhao == null)
            return NotFound(Falha(MensagemNaoEncontrado));

        if (request.PropriedadeId > 0 &&
            request.PropriedadeId != talhao.PropriedadeId)
            return BadRequest(Falha(
                "Não é permitido alterar a propriedade de um talhão."));

        var resultado = await _talhaoService.AtualizarAsync(id, request);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await TalhaoAutorizadoAsync(id) == null)
            return NotFound(Falha(MensagemNaoEncontrado));

        var resultado = await _talhaoService.ExcluirAsync(id);

        return resultado.Success
            ? Ok(resultado)
            : BadRequest(resultado);
    }
}
using System.Security.Claims;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

/// <summary>
/// Funcionários das propriedades do usuário autenticado. O usuário vem SEMPRE do
/// token JWT; a propriedade informada é validada contra ele em todas as operações.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FuncionarioController : ControllerBase
{
    private readonly IFuncionarioService _funcionarioService;

    public FuncionarioController(IFuncionarioService funcionarioService)
    {
        _funcionarioService = funcionarioService;
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out var id) && id > 0 ? id : null;
    }

    // GET /api/Funcionario?propriedadeId=1&somenteAtivos=true
    // Sem propriedadeId: todos os funcionários das propriedades do usuário.
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? propriedadeId,
        [FromQuery] bool somenteAtivos = false)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.ListarAsync(
            propriedadeId, somenteAtivos, usuarioId.Value);

        return resultado.Success ? Ok(resultado) : RespostaDeFalha(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.BuscarPorIdAsync(id, usuarioId.Value);

        return resultado.Success ? Ok(resultado) : RespostaDeFalha(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] FuncionarioRequest request)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.CriarAsync(request, usuarioId.Value);

        if (!resultado.Success) return RespostaDeFalha(resultado);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = resultado.Data!.Id },
            resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] FuncionarioRequest request)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.AtualizarAsync(id, request, usuarioId.Value);

        return resultado.Success ? Ok(resultado) : RespostaDeFalha(resultado);
    }

    // POST /api/Funcionario/{id}/desligar  { dataDesligamento, observacao }
    [HttpPost("{id:int}/desligar")]
    public async Task<IActionResult> Desligar(int id, [FromBody] DesligarFuncionarioRequest request)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.DesligarAsync(id, request, usuarioId.Value);

        return resultado.Success ? Ok(resultado) : RespostaDeFalha(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null) return Unauthorized();

        var resultado = await _funcionarioService.ExcluirAsync(id, usuarioId.Value);

        return resultado.Success ? Ok(resultado) : RespostaDeFalha(resultado);
    }

    // 404 só para "não encontrado" (inclui recurso de outro usuário); o resto é 400.
    private IActionResult RespostaDeFalha<T>(AppResponse<T> resultado)
    {
        if (resultado.Message == IFuncionarioService.MensagemNaoEncontrado
            || resultado.Message == IFuncionarioService.MensagemPropriedadeNaoEncontrada)
        {
            return NotFound(resultado);
        }

        return BadRequest(resultado);
    }
}

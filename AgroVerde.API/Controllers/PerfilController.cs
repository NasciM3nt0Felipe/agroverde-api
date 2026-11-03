
using System.Security.Claims;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class PerfilController : ControllerBase
{
    private readonly IPessoaService _pessoaService;

    public PerfilController(IPessoaService pessoaService)
    {
        _pessoaService = pessoaService;
    }

    // Recupera o ID do usuario autenticado pelo JWT.
    private int? ObterUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(valor, out var usuarioId) && usuarioId > 0)
            return usuarioId;

        return null;
    }

    // GET /api/Perfil
    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized("Token sem identificador de usuario valido.");

        var resultado = await _pessoaService.ObterPerfilAsync(usuarioId.Value);

        if (!resultado.Success)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    // PUT /api/Perfil
    [HttpPut]
    public async Task<IActionResult> Salvar([FromBody] PerfilRequest request)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized("Token sem identificador de usuario valido.");

        var resultado = await _pessoaService.SalvarPerfilAsync(
            usuarioId.Value,
            request
        );

        if (!resultado.Success)
            return BadRequest(resultado);

        return Ok(resultado);
    }
}

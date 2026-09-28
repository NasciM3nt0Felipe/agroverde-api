using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        var resultado = await _usuarioService.RegistrarAsync(request);

        if (!resultado.Success)
        {
            return BadRequest(resultado);
        }

        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _usuarioService.LoginAsync(request);

        if (!resultado.Success)
        {
            return Unauthorized(resultado);
        }

        return Ok(resultado);
    }
}

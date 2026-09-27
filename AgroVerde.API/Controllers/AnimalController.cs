using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    private readonly IAnimalService _service;

    public AnimalController(IAnimalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
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

    [HttpPost]
    public async Task<IActionResult> Inserir([FromBody] AnimalRequest request)
    {
        var result = await _service.InserirAsync(request);
        return CreatedAtAction(nameof(BuscarPorId), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AnimalRequest request)
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

    [HttpPost("pesagens")]
    public async Task<IActionResult> AdicionarPesagem([FromBody] PesagemRequest request)
    {
        var result = await _service.AdicionarPesagemAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("vacinacoes")]
    public async Task<IActionResult> AdicionarVacinacao([FromBody] VacinacaoRequest request)
    {
        var result = await _service.AdicionarVacinacaoAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("sanitario")]
    public async Task<IActionResult> AdicionarControleSanitario([FromBody] ControleSanitarioRequest request)
    {
        var result = await _service.AdicionarControleSanitarioAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reproducao")]
    public async Task<IActionResult> AdicionarRegistroReproducao([FromBody] RegistroReproducaoRequest request)
    {
        var result = await _service.AdicionarRegistroReproducaoAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
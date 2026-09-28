using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var response = await _estoqueService.BuscarPorIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpGet("propriedade/{propriedadeId}")]
    public async Task<IActionResult> ListarPorPropriedade(int propriedadeId)
    {
        var response =
            await _estoqueService.ListarPorPropriedadeIdAsync(propriedadeId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("propriedade/{propriedadeId}/vacinas")]
    public async Task<IActionResult> ListarVacinasDisponiveis(
        int propriedadeId)
    {
        var response =
            await _estoqueService.ListarVacinasDisponiveisAsync(propriedadeId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] EstoqueItemRequest request)
    {
        var response = await _estoqueService.CriarAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = response.Data!.Id },
            response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] EstoqueItemRequest request)
    {
        var response = await _estoqueService.AtualizarAsync(id, request);

        if (!response.Success)
        {
            if (response.Message == "Item de estoque não encontrado.")
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var response = await _estoqueService.ExcluirAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost("{id}/consumir")]
    public async Task<IActionResult> Consumir(
        int id,
        [FromQuery] double quantidade)
    {
        var response =
            await _estoqueService.ConsumirEstoqueAsync(id, quantidade);

        if (!response.Success)
        {
            if (response.Message == "Item de estoque não encontrado.")
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("consumo-safra")]
    public async Task<IActionResult> RegistrarConsumoSafra(
        [FromBody] ConsumoEstoqueRequest request)
    {
        var response =
            await _estoqueService.RegistrarConsumoSafraAsync(request);

        if (!response.Success)
        {
            if (response.Message == "Item de estoque não encontrado.")
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
 
    }
}
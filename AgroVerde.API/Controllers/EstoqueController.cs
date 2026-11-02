
using System.Security.Claims;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;
    private readonly IPropriedadeService _propriedadeService;

    public EstoqueController(
        IEstoqueService estoqueService,
        IPropriedadeService propriedadeService)
    {
        _estoqueService = estoqueService;
        _propriedadeService = propriedadeService;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> TemAcessoPropriedadeAsync(
        int propriedadeId)
    {
        if (propriedadeId <= 0)
            return false;

        var resultado = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, UsuarioId);

        return resultado.Success && resultado.Data != null;
    }

    private async Task<EstoqueItemResponse?> BuscarItemAutorizadoAsync(
        int id)
    {
        var resultado = await _estoqueService.BuscarPorIdAsync(id);

        if (!resultado.Success || resultado.Data == null)
            return null;

        if (!await TemAcessoPropriedadeAsync(
            resultado.Data.PropriedadeId))
            return null;

        return resultado.Data;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        if (await BuscarItemAutorizadoAsync(id) == null)
            return NotFound();

        var response = await _estoqueService.BuscarPorIdAsync(id);
        return Ok(response);
    }

    [HttpGet("propriedade/{propriedadeId:int}")]
    public async Task<IActionResult> ListarPorPropriedade(
        int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        var response = await _estoqueService
            .ListarPorPropriedadeIdAsync(propriedadeId);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpGet("propriedade/{propriedadeId:int}/vacinas")]
    public async Task<IActionResult> ListarVacinasDisponiveis(
        int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        var response = await _estoqueService
            .ListarVacinasDisponiveisAsync(propriedadeId);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] EstoqueItemRequest request)
    {
        if (!await TemAcessoPropriedadeAsync(request.PropriedadeId))
            return NotFound();

        var response = await _estoqueService.CriarAsync(request);

        if (!response.Success || response.Data == null)
            return BadRequest(response);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = response.Data.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] EstoqueItemRequest request)
    {
        var item = await BuscarItemAutorizadoAsync(id);

        if (item == null)
            return NotFound();

        if (request.PropriedadeId > 0 &&
            request.PropriedadeId != item.PropriedadeId)
            return BadRequest(
                "Não é permitido alterar a propriedade do item.");

        var response = await _estoqueService
            .AtualizarAsync(id, request);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await BuscarItemAutorizadoAsync(id) == null)
            return NotFound();

        var response = await _estoqueService.ExcluirAsync(id);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpPost("{id:int}/consumir")]
    public async Task<IActionResult> Consumir(
        int id,
        [FromQuery] double quantidade)
    {
        if (await BuscarItemAutorizadoAsync(id) == null)
            return NotFound();

        var response = await _estoqueService
            .ConsumirEstoqueAsync(id, quantidade);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpPost("consumo-safra")]
    public async Task<IActionResult> RegistrarConsumoSafra(
        [FromBody] ConsumoEstoqueRequest request)
    {
        if (await BuscarItemAutorizadoAsync(
            request.EstoqueItemId) == null)
            return NotFound();

        var response = await _estoqueService
            .RegistrarConsumoSafraAsync(request);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }
}

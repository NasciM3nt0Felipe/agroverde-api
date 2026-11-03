using System.Security.Claims;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using AgroVerde.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AgroVerde.API.Controllers;

// Rebanho: o usuário vem do token e só acessa animais (e seus registros)
// das propriedades dele. Recursos de outros usuários respondem 404.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    private readonly IAnimalService _service;
    private readonly IPropriedadeService _propriedadeService;
    private readonly IMemoryCache _cache;

    public AnimalController(
        IAnimalService service,
        IPropriedadeService propriedadeService,
        IMemoryCache cache)
    {
        _service = service;
        _propriedadeService = propriedadeService;
        _cache = cache;
    }

    private int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> TemAcessoPropriedadeAsync(int propriedadeId)
    {
        if (propriedadeId <= 0)
            return false;

        var resultado = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, UsuarioId);

        return resultado.Success && resultado.Data != null;
    }

    private async Task<int?> PropriedadeDoAnimalAutorizadaAsync(int animalId)
    {
        var propriedadeId = await _service.ObterPropriedadeDoAnimalAsync(animalId);

        return propriedadeId.HasValue &&
               await TemAcessoPropriedadeAsync(propriedadeId.Value)
            ? propriedadeId
            : null;
    }

    private async Task<bool> TemAcessoRegistroAsync(TipoRegistroAnimal tipo, int id)
    {
        var propriedadeId = await _service.ObterPropriedadeDoRegistroAsync(tipo, id);

        return propriedadeId.HasValue &&
               await TemAcessoPropriedadeAsync(propriedadeId.Value);
    }

    // O Dashboard exibe o total de animais.
    private void DescartarCacheDashboard(int propriedadeId) =>
        _cache.Remove(DashboardController.ChaveCache(propriedadeId));

    private static IActionResult Falha<T>(AppResponse<T> resultado) =>
        new BadRequestObjectResult(resultado);

    // GET api/Animal — animais de todas as propriedades do usuário.
    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var propriedades = await _propriedadeService
            .ListarPorUsuarioIdAsync(UsuarioId);

        var animais = new List<AnimalResponse>();

        foreach (var propriedade in propriedades.Data ?? [])
        {
            var resultado = await _service.ListarPorPropriedadeAsync(propriedade.Id);

            if (!resultado.Success)
                return Falha(resultado);

            animais.AddRange(resultado.Data ?? []);
        }

        return Ok(AppResponse<IEnumerable<AnimalResponse>>.Ok(
            "Animais listados com sucesso.",
            animais));
    }

    [HttpGet("propriedade/{propriedadeId:int}")]
    public async Task<IActionResult> ListarPorPropriedade(int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        var result = await _service.ListarPorPropriedadeAsync(propriedadeId);

        return result.Success ? Ok(result) : Falha(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        if (await PropriedadeDoAnimalAutorizadaAsync(id) == null)
            return NotFound();

        var result = await _service.BuscarPorIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Inserir([FromBody] AnimalRequest request)
    {
        if (!await TemAcessoPropriedadeAsync(request.PropriedadeId))
            return NotFound();

        var result = await _service.InserirAsync(request);

        if (!result.Success || result.Data == null)
            return Falha(result);

        DescartarCacheDashboard(request.PropriedadeId);

        return CreatedAtAction(nameof(BuscarPorId), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AnimalRequest request)
    {
        var propriedadeId = await PropriedadeDoAnimalAutorizadaAsync(id);

        if (propriedadeId == null)
            return NotFound();

        if (request.PropriedadeId != propriedadeId)
            return BadRequest(AppResponse<AnimalResponse>.Fail(
                "Não é permitido alterar a propriedade do animal.",
                ["Não é permitido alterar a propriedade do animal."]));

        var result = await _service.AtualizarAsync(id, request);

        if (!result.Success)
            return Falha(result);

        DescartarCacheDashboard(propriedadeId.Value);

        return Ok(result);
    }

    // Exclui o animal e, em cascata, seus registros (as doses já baixadas
    // do estoque não retornam).
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var propriedadeId = await PropriedadeDoAnimalAutorizadaAsync(id);

        if (propriedadeId == null)
            return NotFound();

        var result = await _service.ExcluirAsync(id);

        if (!result.Success)
            return Falha(result);

        DescartarCacheDashboard(propriedadeId.Value);

        return Ok(result);
    }

    // ---------- Listagens por propriedade ----------

    [HttpGet("propriedade/{propriedadeId:int}/pesagens")]
    public async Task<IActionResult> ListarPesagens(int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        return Ok(await _service.ListarPesagensPorPropriedadeAsync(propriedadeId));
    }

    [HttpGet("propriedade/{propriedadeId:int}/vacinacoes")]
    public async Task<IActionResult> ListarVacinacoes(int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        return Ok(await _service.ListarVacinacoesPorPropriedadeAsync(propriedadeId));
    }

    [HttpGet("propriedade/{propriedadeId:int}/sanitario")]
    public async Task<IActionResult> ListarSanitario(int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        return Ok(await _service.ListarControlesSanitariosPorPropriedadeAsync(propriedadeId));
    }

    [HttpGet("propriedade/{propriedadeId:int}/reproducao")]
    public async Task<IActionResult> ListarReproducao(int propriedadeId)
    {
        if (!await TemAcessoPropriedadeAsync(propriedadeId))
            return NotFound();

        return Ok(await _service.ListarReproducoesPorPropriedadeAsync(propriedadeId));
    }

    // ---------- Registros ----------

    [HttpPost("pesagens")]
    public async Task<IActionResult> AdicionarPesagem([FromBody] PesagemRequest request)
    {
        if (await PropriedadeDoAnimalAutorizadaAsync(request.AnimalId) == null)
            return NotFound();

        var result = await _service.AdicionarPesagemAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // Registra a vacinação e baixa 1 dose do estoque na mesma transação.
    [HttpPost("vacinacoes")]
    public async Task<IActionResult> AdicionarVacinacao([FromBody] VacinacaoRequest request)
    {
        if (await PropriedadeDoAnimalAutorizadaAsync(request.AnimalId) == null)
            return NotFound();

        var result = await _service.AdicionarVacinacaoAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("sanitario")]
    public async Task<IActionResult> AdicionarControleSanitario([FromBody] ControleSanitarioRequest request)
    {
        if (await PropriedadeDoAnimalAutorizadaAsync(request.AnimalId) == null)
            return NotFound();

        var result = await _service.AdicionarControleSanitarioAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reproducao")]
    public async Task<IActionResult> AdicionarRegistroReproducao([FromBody] RegistroReproducaoRequest request)
    {
        if (await PropriedadeDoAnimalAutorizadaAsync(request.AnimalId) == null)
            return NotFound();

        var result = await _service.AdicionarRegistroReproducaoAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("pesagens/{id:int}")]
    public Task<IActionResult> ExcluirPesagem(int id) =>
        ExcluirRegistroAsync(TipoRegistroAnimal.Pesagem, id);

    // Exclui só o histórico: a dose aplicada não volta ao estoque.
    [HttpDelete("vacinacoes/{id:int}")]
    public Task<IActionResult> ExcluirVacinacao(int id) =>
        ExcluirRegistroAsync(TipoRegistroAnimal.Vacinacao, id);

    [HttpDelete("sanitario/{id:int}")]
    public Task<IActionResult> ExcluirSanitario(int id) =>
        ExcluirRegistroAsync(TipoRegistroAnimal.Sanitario, id);

    [HttpDelete("reproducao/{id:int}")]
    public Task<IActionResult> ExcluirReproducao(int id) =>
        ExcluirRegistroAsync(TipoRegistroAnimal.Reproducao, id);

    private async Task<IActionResult> ExcluirRegistroAsync(TipoRegistroAnimal tipo, int id)
    {
        if (!await TemAcessoRegistroAsync(tipo, id))
            return NotFound();

        var result = await _service.ExcluirRegistroAsync(tipo, id);

        return result.Success ? Ok(result) : NotFound(result);
    }
}

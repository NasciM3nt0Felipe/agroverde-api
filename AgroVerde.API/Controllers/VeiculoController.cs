using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using AgroVerde.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AgroVerde.API.Controllers
{
    // Veículos, abastecimentos e manutenções. O usuário vem do token e só
    // acessa veículos das propriedades dele; recursos de outros usuários
    // respondem 404. Abastecimentos e manutenções geram a despesa no
    // Financeiro na mesma transação (o app não cria essas despesas).
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculoController : ControllerBase
    {
        private readonly IVeiculoService _veiculoService;
        private readonly IPropriedadeService _propriedadeService;
        private readonly IMemoryCache _cache;

        public VeiculoController(
            IVeiculoService veiculoService,
            IPropriedadeService propriedadeService,
            IMemoryCache cache)
        {
            _veiculoService = veiculoService;
            _propriedadeService = propriedadeService;
            _cache = cache;
        }

        private int UsuarioId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<bool> TemAcessoPropriedadeAsync(int propriedadeId)
        {
            if (propriedadeId <= 0)
                return false;

            var resultado = await _propriedadeService.BuscarPorIdAsync(propriedadeId, UsuarioId);

            return resultado.Success && resultado.Data != null;
        }

        // Propriedade do recurso, se pertencer ao usuário; senão null.
        private async Task<int?> AutorizarAsync(int? propriedadeId) =>
            propriedadeId.HasValue && await TemAcessoPropriedadeAsync(propriedadeId.Value)
                ? propriedadeId
                : null;

        // O Dashboard soma as despesas do Financeiro.
        private void DescartarCacheDashboard(int propriedadeId) =>
            _cache.Remove(DashboardController.ChaveCache(propriedadeId));

        private static IActionResult Erro500(string contexto, Exception ex) =>
            new ObjectResult(AppResponse<string>.Fail($"Erro interno ao {contexto}: {ex.Message}"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

        private IActionResult DadosInvalidos()
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();

            return BadRequest(AppResponse<string>.Fail(
                erros.FirstOrDefault() ?? "Dados inválidos.", erros));
        }

        /// <summary>Veículos de todas as propriedades do usuário.</summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var propriedades = await _propriedadeService.ListarPorUsuarioIdAsync(UsuarioId);
                var lista = new List<VeiculoResponse>();

                foreach (var propriedade in propriedades.Data ?? [])
                    lista.AddRange((await _veiculoService.ObterPorPropriedadeAsync(propriedade.Id)).Data ?? []);

                return Ok(AppResponse<IEnumerable<VeiculoResponse>>.Ok("Veículos recuperados com sucesso.", lista));
            }
            catch (Exception ex)
            {
                return Erro500("buscar veículos", ex);
            }
        }

        /// <summary>Veículos da propriedade, com totais de abastecimento e manutenção.</summary>
        [HttpGet("propriedade/{propriedadeId:int}")]
        public async Task<IActionResult> ObterPorPropriedade(int propriedadeId)
        {
            if (!await TemAcessoPropriedadeAsync(propriedadeId))
                return NotFound();

            return Ok(await _veiculoService.ObterPorPropriedadeAsync(propriedadeId));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            if (await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(id)) == null)
                return NotFound();

            var resultado = await _veiculoService.ObterPorIdAsync(id);
            return resultado.Success ? Ok(resultado) : NotFound(resultado);
        }

        /// <summary>Veículos do usuário com o status informado.</summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> ObterPorStatus(StatusVeiculo status)
        {
            var propriedades = await _propriedadeService.ListarPorUsuarioIdAsync(UsuarioId);
            var lista = new List<VeiculoResponse>();

            foreach (var propriedade in propriedades.Data ?? [])
                lista.AddRange(((await _veiculoService.ObterPorPropriedadeAsync(propriedade.Id)).Data ?? [])
                    .Where(v => v.Status == status));

            return Ok(AppResponse<IEnumerable<VeiculoResponse>>.Ok("Veículos filtrados com sucesso.", lista));
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] VeiculoRequest request)
        {
            if (!ModelState.IsValid)
                return DadosInvalidos();

            if (!await TemAcessoPropriedadeAsync(request.PropriedadeId))
                return NotFound();

            try
            {
                var resultado = await _veiculoService.CriarAsync(request);
                if (!resultado.Success)
                    return BadRequest(resultado);

                return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Data!.Id }, resultado);
            }
            catch (Exception ex)
            {
                return Erro500("cadastrar veículo", ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] VeiculoRequest request)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(id));
            if (propriedadeId == null)
                return NotFound();

            if (!ModelState.IsValid)
                return DadosInvalidos();

            if (request.PropriedadeId != propriedadeId)
                return BadRequest(AppResponse<string>.Fail("Não é permitido alterar a propriedade do veículo."));

            try
            {
                var resultado = await _veiculoService.AtualizarAsync(id, request);
                return resultado.Success ? Ok(resultado) : BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return Erro500("atualizar veículo", ex);
            }
        }

        [HttpPatch("{id:int}/hodometro")]
        public async Task<IActionResult> AtualizarHodometro(int id, [FromBody] AtualizarHodometroRequest request)
        {
            if (await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(id)) == null)
                return NotFound();

            if (!ModelState.IsValid)
                return DadosInvalidos();

            var resultado = await _veiculoService.AtualizarHodometroAsync(id, request);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        /// <summary>Remove o veículo (recusado se houver abastecimentos ou manutenções).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remover(int id)
        {
            if (await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(id)) == null)
                return NotFound();

            var resultado = await _veiculoService.RemoverAsync(id);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        // ---------- Abastecimentos ----------

        [HttpGet("{veiculoId:int}/abastecimentos")]
        public async Task<IActionResult> ListarAbastecimentos(int veiculoId)
        {
            if (await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(veiculoId)) == null)
                return NotFound();

            return Ok(await _veiculoService.ListarAbastecimentosAsync(veiculoId));
        }

        [HttpPost("abastecimentos")]
        public async Task<IActionResult> CriarAbastecimento([FromBody] AbastecimentoRequest request)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(request.VeiculoId));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.CriarAbastecimentoAsync(request);
            if (!resultado.Success)
                return BadRequest(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }

        [HttpPut("abastecimentos/{id:int}")]
        public async Task<IActionResult> AtualizarAbastecimento(int id, [FromBody] AbastecimentoRequest request)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDoAbastecimentoAsync(id));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.AtualizarAbastecimentoAsync(id, request);
            if (!resultado.Success)
                return BadRequest(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }

        /// <summary>Exclui o abastecimento e a despesa gerada por ele.</summary>
        [HttpDelete("abastecimentos/{id:int}")]
        public async Task<IActionResult> ExcluirAbastecimento(int id)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDoAbastecimentoAsync(id));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.ExcluirAbastecimentoAsync(id);
            if (!resultado.Success)
                return NotFound(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }

        // ---------- Manutenções ----------

        [HttpGet("{veiculoId:int}/manutencoes")]
        public async Task<IActionResult> ListarManutencoes(int veiculoId)
        {
            if (await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(veiculoId)) == null)
                return NotFound();

            return Ok(await _veiculoService.ListarManutencoesAsync(veiculoId));
        }

        [HttpPost("manutencoes")]
        public async Task<IActionResult> CriarManutencao([FromBody] ManutencaoRequest request)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDoVeiculoAsync(request.VeiculoId));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.CriarManutencaoAsync(request);
            if (!resultado.Success)
                return BadRequest(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }

        [HttpPut("manutencoes/{id:int}")]
        public async Task<IActionResult> AtualizarManutencao(int id, [FromBody] ManutencaoRequest request)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDaManutencaoAsync(id));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.AtualizarManutencaoAsync(id, request);
            if (!resultado.Success)
                return BadRequest(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }

        /// <summary>Exclui a manutenção e a despesa gerada por ela.</summary>
        [HttpDelete("manutencoes/{id:int}")]
        public async Task<IActionResult> ExcluirManutencao(int id)
        {
            var propriedadeId = await AutorizarAsync(await _veiculoService.ObterPropriedadeDaManutencaoAsync(id));
            if (propriedadeId == null)
                return NotFound();

            var resultado = await _veiculoService.ExcluirManutencaoAsync(id);
            if (!resultado.Success)
                return NotFound(resultado);

            DescartarCacheDashboard(propriedadeId.Value);
            return Ok(resultado);
        }
    }
}

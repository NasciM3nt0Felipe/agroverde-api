using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Application.Services;
using AgroVerde.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgroVerde.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculoController : ControllerBase
    {
        private readonly IVeiculoService _veiculoService;

        public VeiculoController(IVeiculoService veiculoService)
        {
            _veiculoService = veiculoService;
        }

        /// <summary>
        /// Lista todos os veículos e maquinários cadastrados.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AppResponse<IEnumerable<VeiculoResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var resultado = await _veiculoService.ObterTodosAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao buscar veículos: {ex.Message}"));
            }
        }

        /// <summary>
        /// Busca um veículo pelo seu identificador (ID).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AppResponse<VeiculoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                var resultado = await _veiculoService.ObterPorIdAsync(id);
                if (!resultado.Success)
                    return NotFound(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao buscar veículo: {ex.Message}"));
            }
        }

        /// <summary>
        /// Filtra veículos pelo status de operação.
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(AppResponse<IEnumerable<VeiculoResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorStatus(StatusVeiculo status)
        {
            try
            {
                var resultado = await _veiculoService.ObterPorStatusAsync(status);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro ao filtrar veículos por status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cadastra um novo veículo/maquinário.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AppResponse<VeiculoResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Criar([FromBody] VeiculoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(AppResponse<string>.Fail("Dados inválidos fornecidos para cadastro."));

                var resultado = await _veiculoService.CriarAsync(request);
                if (!resultado.Success)
                    return BadRequest(resultado);

                return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Data!.Id }, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao cadastrar veículo: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza os dados de um veículo existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AppResponse<VeiculoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] VeiculoRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                if (!ModelState.IsValid)
                    return BadRequest(AppResponse<string>.Fail("Dados inválidos fornecidos para atualização."));

                var resultado = await _veiculoService.AtualizarAsync(id, request);
                if (!resultado.Success)
                {
                    if (resultado.Message != null && resultado.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                        return NotFound(resultado);

                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao atualizar veículo: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza pontualmente o horímetro e a quilometragem do veículo.
        /// </summary>
        [HttpPatch("{id:int}/hodometro")]
        [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AtualizarHodometro(int id, [FromBody] AtualizarHodometroRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                if (!ModelState.IsValid)
                    return BadRequest(AppResponse<string>.Fail("Dados de horímetro/quilometragem inválidos."));

                var resultado = await _veiculoService.AtualizarHodometroAsync(id, request);
                if (!resultado.Success)
                {
                    if (resultado.Message != null && resultado.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                        return NotFound(resultado);

                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao atualizar medidores: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove um veículo da base de dados.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remover(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                var resultado = await _veiculoService.RemoverAsync(id);
                if (!resultado.Success)
                    return NotFound(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao remover veículo: {ex.Message}"));
            }
        }
    }
}
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
    public class PessoaController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;

        public PessoaController(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService;
        }

        /// <summary>
        /// Lista todas as pessoas cadastradas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AppResponse<IEnumerable<PessoaResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var resultado = await _pessoaService.ObterTodosAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao buscar pessoas: {ex.Message}"));
            }
        }

        /// <summary>
        /// Busca uma pessoa por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AppResponse<PessoaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                var resultado = await _pessoaService.ObterPorIdAsync(id);
                if (!resultado.Success)
                    return NotFound(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao obter pessoa: {ex.Message}"));
            }
        }

        /// <summary>
        /// Filtra pessoas pelo tipo (ex: Funcionário, Fornecedor, Veterinário).
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(typeof(AppResponse<IEnumerable<PessoaResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPorTipo(TipoPessoa tipo)
        {
            try
            {
                var resultado = await _pessoaService.ObterPorTipoAsync(tipo);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao filtrar pessoas: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cadastra uma nova pessoa.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AppResponse<PessoaResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Criar([FromBody] PessoaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(AppResponse<string>.Fail("Dados inválidos fornecidos para cadastro."));

                var resultado = await _pessoaService.CriarAsync(request);
                if (!resultado.Success)
                    return BadRequest(resultado);

                return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Data!.Id }, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao criar pessoa: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza os dados de uma pessoa.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AppResponse<PessoaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] PessoaRequest request)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                if (!ModelState.IsValid)
                    return BadRequest(AppResponse<string>.Fail("Dados inválidos fornecidos para atualização."));

                var resultado = await _pessoaService.AtualizarAsync(id, request);
                if (!resultado.Success)
                {
                    if (resultado.Message != null && resultado.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase))
                        return NotFound(resultado);

                    return BadRequest(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao atualizar pessoa: {ex.Message}"));
            }
        }

        /// <summary>
        /// Ativa ou inativa o cadastro de uma pessoa.
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AlterarStatus(int id, [FromQuery] bool ativo)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(AppResponse<string>.Fail("O ID informado deve ser maior que zero."));

                var resultado = await _pessoaService.AlterarStatusAsync(id, ativo);
                if (!resultado.Success)
                    return NotFound(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao alterar status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove uma pessoa da base de dados.
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

                var resultado = await _pessoaService.RemoverAsync(id);
                if (!resultado.Success)
                    return NotFound(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AppResponse<string>.Fail($"Erro interno ao remover pessoa: {ex.Message}"));
            }
        }
    }
}
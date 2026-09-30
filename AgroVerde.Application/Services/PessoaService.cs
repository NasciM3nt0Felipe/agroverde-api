using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services
{
    public class PessoaService : IPessoaService
    {
        private readonly IPessoaRepository _pessoaRepository;

        public PessoaService(IPessoaRepository pessoaRepository)
        {
            _pessoaRepository = pessoaRepository;
        }

        public async Task<AppResponse<IEnumerable<PessoaResponse>>> ObterTodosAsync()
        {
            var lista = await _pessoaRepository.GetAllAsync();
            var dtos = lista.Select(MapearParaResponse);
            return AppResponse<IEnumerable<PessoaResponse>>.Ok("Pessoas recuperadas com sucesso.", dtos);
        }

        public async Task<AppResponse<PessoaResponse>> ObterPorIdAsync(int id)
        {
            if (id <= 0)
                return AppResponse<PessoaResponse>.Fail("Identificador inválido.");

            var pessoa = await _pessoaRepository.GetByIdAsync(id);
            if (pessoa == null)
                return AppResponse<PessoaResponse>.Fail("Pessoa não encontrada.");

            return AppResponse<PessoaResponse>.Ok("Pessoa recuperada com sucesso.", MapearParaResponse(pessoa));
        }

        public async Task<AppResponse<IEnumerable<PessoaResponse>>> ObterPorTipoAsync(TipoPessoa tipo)
        {
            var lista = await _pessoaRepository.ObterPorTipoAsync(tipo);
            var dtos = lista.Select(MapearParaResponse);
            return AppResponse<IEnumerable<PessoaResponse>>.Ok("Pessoas filtradas com sucesso.", dtos);
        }

        public async Task<AppResponse<PessoaResponse>> CriarAsync(PessoaRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nome) || req.Nome.Trim().Length < 3)
                return AppResponse<PessoaResponse>.Fail("O nome deve conter ao menos 3 caracteres.");

            // Regra de Negócio: Cargo obrigatório para funcionários e técnicos
            if ((req.Tipo == TipoPessoa.Funcionario || req.Tipo == TipoPessoa.Veterinario || req.Tipo == TipoPessoa.Agronomo)
                && string.IsNullOrWhiteSpace(req.CargoOuFuncao))
            {
                return AppResponse<PessoaResponse>.Fail($"O campo Cargo/Função é obrigatório para {req.Tipo}.");
            }

            // Regra de Negócio: Higienização e Unicidade de CPF/CNPJ
            string? docLimpo = null;
            if (!string.IsNullOrWhiteSpace(req.CpfCnpj))
            {
                docLimpo = Regex.Replace(req.CpfCnpj, @"[^\d]", "");
                if (docLimpo.Length != 11 && docLimpo.Length != 14)
                    return AppResponse<PessoaResponse>.Fail("Documento inválido. O CPF deve ter 11 dígitos ou o CNPJ deve ter 14 dígitos.");

                var docExistente = await _pessoaRepository.ObterPorDocumentoAsync(docLimpo);
                if (docExistente != null)
                    return AppResponse<PessoaResponse>.Fail("Já existe uma pessoa cadastrada com este CPF/CNPJ.");
            }

            // Regra de Negócio: Unicidade de E-mail
            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                var emailExistente = await _pessoaRepository.ObterPorEmailAsync(req.Email.Trim());
                if (emailExistente != null)
                    return AppResponse<PessoaResponse>.Fail("Já existe uma pessoa cadastrada com este e-mail.");
            }

            var pessoa = new Pessoa
            {
                Nome = req.Nome.Trim(),
                CpfCnpj = docLimpo,
                Tipo = req.Tipo,
                Telefone = req.Telefone?.Trim(),
                Email = req.Email?.Trim().ToLower(),
                CargoOuFuncao = req.CargoOuFuncao?.Trim(),
                Ativo = req.Ativo,
                DataCadastro = DateTime.UtcNow,
                Observacoes = req.Observacoes?.Trim()
            };

            await _pessoaRepository.AddAsync(pessoa);
            await _pessoaRepository.SaveChangesAsync();

            return AppResponse<PessoaResponse>.Ok("Pessoa cadastrada com sucesso.", MapearParaResponse(pessoa));
        }

        public async Task<AppResponse<PessoaResponse>> AtualizarAsync(int id, PessoaRequest req)
        {
            if (id <= 0)
                return AppResponse<PessoaResponse>.Fail("Identificador inválido.");

            var pessoa = await _pessoaRepository.GetByIdAsync(id);
            if (pessoa == null)
                return AppResponse<PessoaResponse>.Fail("Pessoa não encontrada.");

            if (string.IsNullOrWhiteSpace(req.Nome) || req.Nome.Trim().Length < 3)
                return AppResponse<PessoaResponse>.Fail("O nome deve conter ao menos 3 caracteres.");

            if ((req.Tipo == TipoPessoa.Funcionario || req.Tipo == TipoPessoa.Veterinario || req.Tipo == TipoPessoa.Agronomo)
                && string.IsNullOrWhiteSpace(req.CargoOuFuncao))
            {
                return AppResponse<PessoaResponse>.Fail($"O campo Cargo/Função é obrigatório para {req.Tipo}.");
            }

            string? docLimpo = null;
            if (!string.IsNullOrWhiteSpace(req.CpfCnpj))
            {
                docLimpo = Regex.Replace(req.CpfCnpj, @"[^\d]", "");
                if (docLimpo.Length != 11 && docLimpo.Length != 14)
                    return AppResponse<PessoaResponse>.Fail("Documento inválido. O CPF deve ter 11 dígitos ou o CNPJ deve ter 14 dígitos.");

                var docExistente = await _pessoaRepository.ObterPorDocumentoAsync(docLimpo);
                if (docExistente != null && docExistente.Id != id)
                    return AppResponse<PessoaResponse>.Fail("Este CPF/CNPJ já está em uso por outra pessoa.");
            }

            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                var emailExistente = await _pessoaRepository.ObterPorEmailAsync(req.Email.Trim());
                if (emailExistente != null && emailExistente.Id != id)
                    return AppResponse<PessoaResponse>.Fail("Este e-mail já está em uso por outra pessoa.");
            }

            pessoa.Nome = req.Nome.Trim();
            pessoa.CpfCnpj = docLimpo;
            pessoa.Tipo = req.Tipo;
            pessoa.Telefone = req.Telefone?.Trim();
            pessoa.Email = req.Email?.Trim().ToLower();
            pessoa.CargoOuFuncao = req.CargoOuFuncao?.Trim();
            pessoa.Ativo = req.Ativo;
            pessoa.Observacoes = req.Observacoes?.Trim();

            _pessoaRepository.Update(pessoa);
            await _pessoaRepository.SaveChangesAsync();

            return AppResponse<PessoaResponse>.Ok("Pessoa atualizada com sucesso.", MapearParaResponse(pessoa));
        }

        public async Task<AppResponse<bool>> AlterarStatusAsync(int id, bool ativo)
        {
            if (id <= 0)
                return AppResponse<bool>.Fail("Identificador inválido.");

            var pessoa = await _pessoaRepository.GetByIdAsync(id);
            if (pessoa == null)
                return AppResponse<bool>.Fail("Pessoa não encontrada.");

            pessoa.Ativo = ativo;
            _pessoaRepository.Update(pessoa);
            await _pessoaRepository.SaveChangesAsync();

            string statusTexto = ativo ? "ativada" : "inativada";
            return AppResponse<bool>.Ok($"Pessoa {statusTexto} com sucesso.", true);
        }

        public async Task<AppResponse<bool>> RemoverAsync(int id)
        {
            if (id <= 0)
                return AppResponse<bool>.Fail("Identificador inválido.");

            var pessoa = await _pessoaRepository.GetByIdAsync(id);
            if (pessoa == null)
                return AppResponse<bool>.Fail("Pessoa não encontrada.");

            _pessoaRepository.Delete(pessoa);
            await _pessoaRepository.SaveChangesAsync();

            return AppResponse<bool>.Ok("Pessoa removida com sucesso.", true);
        }

        private static PessoaResponse MapearParaResponse(Pessoa p) => new()
        {
            Id = p.Id,
            Nome = p.Nome,
            CpfCnpj = p.CpfCnpj,
            Tipo = p.Tipo,
            TipoDescricao = p.Tipo.ToString(),
            Telefone = p.Telefone,
            Email = p.Email,
            CargoOuFuncao = p.CargoOuFuncao,
            Ativo = p.Ativo,
            DataCadastro = p.DataCadastro,
            Observacoes = p.Observacoes
        };
    }
}
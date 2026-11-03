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

        // ---------------------------------------------------------------
        // Perfil do usuário autenticado
        // ---------------------------------------------------------------

        public async Task<AppResponse<PerfilResponse?>> ObterPerfilAsync(int usuarioId)
        {
            if (usuarioId <= 0)
                return AppResponse<PerfilResponse?>.Fail("Usuário inválido.");

            var perfil = await _pessoaRepository.ObterPerfilPorUsuarioIdAsync(usuarioId);

            // Usuário sem perfil cadastrado é um estado normal: sucesso com data nula.
            if (perfil == null)
                return AppResponse<PerfilResponse?>.Ok("Perfil ainda não cadastrado.", null);

            return AppResponse<PerfilResponse?>.Ok("Perfil recuperado com sucesso.", MapearPerfil(perfil));
        }

        public async Task<AppResponse<PerfilResponse>> SalvarPerfilAsync(int usuarioId, PerfilRequest req)
        {
            if (usuarioId <= 0)
                return AppResponse<PerfilResponse>.Fail("Usuário inválido.");

            var nome = req.Nome?.Trim() ?? string.Empty;

            if (nome.Length == 0)
                return AppResponse<PerfilResponse>.Fail("Informe o nome.");

            // CPF: mesma regra do app (11 dígitos). Guardado só com dígitos em CpfCnpj.
            string? cpf = null;
            if (!string.IsNullOrWhiteSpace(req.Cpf))
            {
                cpf = Regex.Replace(req.Cpf, @"[^\d]", "");
                if (cpf.Length != 11)
                    return AppResponse<PerfilResponse>.Fail("CPF inválido. Informe 11 dígitos.");
            }

            // CEP: se informado, 8 dígitos (o formato digitado é preservado).
            var cep = LimparOpcional(req.Cep);
            if (cep != null && Regex.Replace(cep, @"[^\d]", "").Length != 8)
                return AppResponse<PerfilResponse>.Fail("CEP inválido. Informe 8 dígitos.");

            // CPF único: nenhum outro registro (perfil ou pessoa operacional) pode usá-lo.
            if (cpf != null)
            {
                var doc = await _pessoaRepository.ObterPorDocumentoAsync(cpf);
                if (doc != null && doc.UsuarioId != usuarioId)
                    return AppResponse<PerfilResponse>.Fail("Este CPF já está cadastrado para outra pessoa.");
            }

            try
            {
                var perfil = await _pessoaRepository.ObterPerfilPorUsuarioIdAsync(usuarioId);
                var novo = perfil == null;

                if (novo)
                {
                    perfil = new Pessoa
                    {
                        UsuarioId = usuarioId,
                        Tipo = TipoPessoa.Outro,
                        Ativo = true,
                        DataCadastro = DateTime.UtcNow
                    };
                }

                perfil!.Nome = nome;
                perfil.CpfCnpj = cpf;
                perfil.Telefone = LimparOpcional(req.Telefone);
                perfil.Cep = cep;
                perfil.Rua = LimparOpcional(req.Rua);
                perfil.Numero = LimparOpcional(req.Numero);
                perfil.Bairro = LimparOpcional(req.Bairro);
                perfil.Cidade = LimparOpcional(req.Cidade);
                perfil.Estado = LimparOpcional(req.Estado);

                if (novo)
                    await _pessoaRepository.AddAsync(perfil);
                else
                    _pessoaRepository.Update(perfil);

                await _pessoaRepository.SaveChangesAsync();

                return AppResponse<PerfilResponse>.Ok(
                    novo ? "Perfil criado com sucesso." : "Perfil atualizado com sucesso.",
                    MapearPerfil(perfil));
            }
            catch (Exception ex)
            {
                // Inclui o caso raro de duas gravações simultâneas do mesmo usuário
                // (o índice único impede um segundo perfil).
                return AppResponse<PerfilResponse>.Fail("Erro ao salvar o perfil.", ex);
            }
        }

        private static string? LimparOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static PerfilResponse MapearPerfil(Pessoa p) => new()
        {
            Id = p.Id,
            UsuarioId = p.UsuarioId ?? 0,
            Nome = p.Nome,
            Cpf = p.CpfCnpj,
            Telefone = p.Telefone,
            Cep = p.Cep,
            Rua = p.Rua,
            Numero = p.Numero,
            Bairro = p.Bairro,
            Cidade = p.Cidade,
            Estado = p.Estado
        };

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

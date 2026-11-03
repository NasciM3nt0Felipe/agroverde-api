using System.Text.RegularExpressions;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class FuncionarioService : IFuncionarioService
{
    private const string MsgCpfJaCadastrado = "CPF já cadastrado.";

    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IPropriedadeRepository _propriedadeRepository;

    public FuncionarioService(
        IFuncionarioRepository funcionarioRepository,
        IPropriedadeRepository propriedadeRepository)
    {
        _funcionarioRepository = funcionarioRepository;
        _propriedadeRepository = propriedadeRepository;
    }

    public async Task<AppResponse<List<FuncionarioResponse>>> ListarAsync(
        int? propriedadeId,
        bool somenteAtivos,
        int usuarioId)
    {
        try
        {
            List<int> ids;

            if (propriedadeId.HasValue)
            {
                if (propriedadeId.Value <= 0)
                {
                    return AppResponse<List<FuncionarioResponse>>.Fail("Propriedade inválida.");
                }

                if (!await PropriedadeDoUsuarioAsync(propriedadeId.Value, usuarioId))
                {
                    return AppResponse<List<FuncionarioResponse>>.Fail(
                        IFuncionarioService.MensagemPropriedadeNaoEncontrada);
                }

                ids = new List<int> { propriedadeId.Value };
            }
            else
            {
                var propriedades = await _propriedadeRepository.ListarPorUsuarioIdAsync(usuarioId);
                ids = propriedades.Select(p => p.Id).ToList();
            }

            var funcionarios = await _funcionarioRepository
                .ListarPorPropriedadesAsync(ids, somenteAtivos);

            return AppResponse<List<FuncionarioResponse>>.Ok(
                "Funcionários listados com sucesso.",
                funcionarios.Select(ParaResponse).ToList());
        }
        catch (Exception ex)
        {
            return AppResponse<List<FuncionarioResponse>>.Fail(
                "Erro ao listar os funcionários.", ex);
        }
    }

    public async Task<AppResponse<FuncionarioResponse>> BuscarPorIdAsync(int id, int usuarioId)
    {
        try
        {
            var funcionario = await ObterDoUsuarioAsync(id, usuarioId);

            if (funcionario == null)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    IFuncionarioService.MensagemNaoEncontrado);
            }

            return AppResponse<FuncionarioResponse>.Ok(
                "Funcionário encontrado com sucesso.", ParaResponse(funcionario));
        }
        catch (Exception ex)
        {
            return AppResponse<FuncionarioResponse>.Fail("Erro ao buscar o funcionário.", ex);
        }
    }

    public async Task<AppResponse<FuncionarioResponse>> CriarAsync(
        FuncionarioRequest request,
        int usuarioId)
    {
        try
        {
            if (request.PropriedadeId <= 0)
            {
                return AppResponse<FuncionarioResponse>.Fail("Propriedade inválida.");
            }

            // A propriedade tem de ser do usuário do token.
            if (!await PropriedadeDoUsuarioAsync(request.PropriedadeId, usuarioId))
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    IFuncionarioService.MensagemPropriedadeNaoEncontrada);
            }

            var dados = Validar(request, out var erro);

            if (dados == null)
            {
                return AppResponse<FuncionarioResponse>.Fail(erro!);
            }

            // Descobre se reaproveita uma Pessoa existente (mesmo CPF) ou cria uma nova.
            Pessoa? pessoa = null;

            if (dados.Cpf != null)
            {
                var existente = await _funcionarioRepository
                    .BuscarPessoaPorDocumentoAsync(dados.Cpf);

                if (existente != null)
                {
                    // Só reaproveita pessoa que já é funcionária (e só de propriedades deste usuário).
                    // Perfil, pessoa avulsa ou pessoa de outro usuário => CPF indisponível.
                    var reaproveitavel =
                        existente.UsuarioId == null
                        && await _funcionarioRepository.ContarVinculosDaPessoaAsync(existente.Id) > 0
                        && !await _funcionarioRepository
                            .PessoaVinculadaAOutroUsuarioAsync(existente.Id, usuarioId);

                    if (!reaproveitavel)
                    {
                        return AppResponse<FuncionarioResponse>.Fail(MsgCpfJaCadastrado);
                    }

                    if (await _funcionarioRepository
                        .ExisteVinculoAsync(existente.Id, request.PropriedadeId))
                    {
                        return AppResponse<FuncionarioResponse>.Fail(
                            "Essa pessoa já está cadastrada como funcionário nesta propriedade.");
                    }

                    pessoa = existente;
                }
            }

            if (pessoa == null)
            {
                pessoa = new Pessoa
                {
                    Tipo = TipoPessoa.Funcionario,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                };
            }

            AplicarPessoa(pessoa, dados);

            var funcionario = new Funcionario
            {
                Pessoa = pessoa,
                PropriedadeId = request.PropriedadeId
            };

            AplicarVinculo(funcionario, dados);

            // Pessoa + vínculo são gravados juntos (uma única transação).
            await _funcionarioRepository.AdicionarAsync(funcionario);
            await _funcionarioRepository.SalvarAsync();

            return AppResponse<FuncionarioResponse>.Ok(
                "Funcionário cadastrado com sucesso.", ParaResponse(funcionario));
        }
        catch (Exception ex)
        {
            return AppResponse<FuncionarioResponse>.Fail("Erro ao cadastrar o funcionário.", ex);
        }
    }

    public async Task<AppResponse<FuncionarioResponse>> AtualizarAsync(
        int id,
        FuncionarioRequest request,
        int usuarioId)
    {
        try
        {
            var funcionario = await ObterDoUsuarioAsync(id, usuarioId);

            if (funcionario == null)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    IFuncionarioService.MensagemNaoEncontrado);
            }

            // A propriedade de um funcionário não muda (omitida/0 mantém a atual).
            if (request.PropriedadeId > 0 && request.PropriedadeId != funcionario.PropriedadeId)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    "Não é permitido alterar a propriedade de um funcionário.");
            }

            var dados = Validar(request, out var erro);

            if (dados == null)
            {
                return AppResponse<FuncionarioResponse>.Fail(erro!);
            }

            var pessoa = funcionario.Pessoa!;

            // Troca de CPF: o novo CPF não pode pertencer a outra pessoa.
            if (dados.Cpf != null && dados.Cpf != pessoa.CpfCnpj)
            {
                var outra = await _funcionarioRepository.BuscarPessoaPorDocumentoAsync(dados.Cpf);

                if (outra != null && outra.Id != pessoa.Id)
                {
                    return AppResponse<FuncionarioResponse>.Fail(MsgCpfJaCadastrado);
                }
            }

            AplicarPessoa(pessoa, dados);
            AplicarVinculo(funcionario, dados);

            await _funcionarioRepository.SalvarAsync();

            return AppResponse<FuncionarioResponse>.Ok(
                "Funcionário atualizado com sucesso.", ParaResponse(funcionario));
        }
        catch (Exception ex)
        {
            return AppResponse<FuncionarioResponse>.Fail("Erro ao atualizar o funcionário.", ex);
        }
    }

    public async Task<AppResponse<FuncionarioResponse>> DesligarAsync(
        int id,
        DesligarFuncionarioRequest request,
        int usuarioId)
    {
        try
        {
            var funcionario = await ObterDoUsuarioAsync(id, usuarioId);

            if (funcionario == null)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    IFuncionarioService.MensagemNaoEncontrado);
            }

            if (!request.DataDesligamento.HasValue)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    "Informe a data de desligamento do funcionário.");
            }

            if (request.Observacao != null && request.Observacao.Trim().Length > 500)
            {
                return AppResponse<FuncionarioResponse>.Fail(
                    "A observação deve ter no máximo 500 caracteres.");
            }

            funcionario.Status = StatusFuncionario.Desligado;
            funcionario.DataDesligamento = request.DataDesligamento.Value.Date;

            if (request.Observacao != null)
            {
                funcionario.Observacao = LimparOpcional(request.Observacao);
            }

            await _funcionarioRepository.SalvarAsync();

            return AppResponse<FuncionarioResponse>.Ok(
                "Funcionário desligado com sucesso.", ParaResponse(funcionario));
        }
        catch (Exception ex)
        {
            return AppResponse<FuncionarioResponse>.Fail("Erro ao desligar o funcionário.", ex);
        }
    }

    public async Task<AppResponse<bool>> ExcluirAsync(int id, int usuarioId)
    {
        try
        {
            var funcionario = await ObterDoUsuarioAsync(id, usuarioId);

            if (funcionario == null)
            {
                return AppResponse<bool>.Fail(IFuncionarioService.MensagemNaoEncontrado);
            }

            var pessoa = funcionario.Pessoa;

            _funcionarioRepository.Remover(funcionario);

            // Se era o único vínculo da pessoa (e ela foi criada como funcionária), remove também
            // a Pessoa, para não deixar dados pessoais órfãos nem travar o CPF para sempre.
            if (pessoa != null
                && pessoa.UsuarioId == null
                && pessoa.Tipo == TipoPessoa.Funcionario
                && await _funcionarioRepository.ContarVinculosDaPessoaAsync(pessoa.Id) <= 1)
            {
                _funcionarioRepository.RemoverPessoa(pessoa);
            }

            await _funcionarioRepository.SalvarAsync();

            return AppResponse<bool>.Ok("Funcionário excluído com sucesso.", true);
        }
        catch (Exception ex)
        {
            return AppResponse<bool>.Fail("Erro ao excluir o funcionário.", ex);
        }
    }

    // ---------------------------------------------------------------
    // Auxiliares
    // ---------------------------------------------------------------

    private async Task<bool> PropriedadeDoUsuarioAsync(int propriedadeId, int usuarioId)
    {
        var propriedade = await _propriedadeRepository.BuscarPorIdAsync(propriedadeId);

        return propriedade != null && propriedade.UsuarioId == usuarioId;
    }

    // Funcionário só é visível se a propriedade dele for do usuário do token.
    private async Task<Funcionario?> ObterDoUsuarioAsync(int id, int usuarioId)
    {
        if (id <= 0)
        {
            return null;
        }

        var funcionario = await _funcionarioRepository.BuscarPorIdAsync(id);

        if (funcionario == null
            || !await PropriedadeDoUsuarioAsync(funcionario.PropriedadeId, usuarioId))
        {
            return null;
        }

        return funcionario;
    }

    private sealed class DadosValidados
    {
        public string Nome { get; init; } = string.Empty;
        public string? Cpf { get; init; }
        public string? Telefone { get; init; }
        public string Cargo { get; init; } = string.Empty;
        public decimal? Salario { get; init; }
        public DateTime? DataContratacao { get; init; }
        public DateTime? DataDesligamento { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? Observacao { get; init; }
    }

    // Regras que vieram do FuncionarioService do Flutter (cargo, salário, status,
    // data de desligamento), mais as validações de tamanho/CPF.
    private static DadosValidados? Validar(FuncionarioRequest r, out string? erro)
    {
        erro = null;

        var nome = r.Nome?.Trim() ?? string.Empty;

        if (nome.Length == 0)
        {
            erro = "Informe o nome.";
            return null;
        }

        if (nome.Length > 150)
        {
            erro = "O nome deve ter no máximo 150 caracteres.";
            return null;
        }

        string? cpf = null;

        if (!string.IsNullOrWhiteSpace(r.Cpf))
        {
            cpf = Regex.Replace(r.Cpf, @"[^\d]", "");

            if (cpf.Length != 11)
            {
                erro = "CPF inválido. Informe 11 dígitos.";
                return null;
            }
        }

        var telefone = LimparOpcional(r.Telefone);

        if (telefone != null && telefone.Length > 20)
        {
            erro = "O telefone deve ter no máximo 20 caracteres.";
            return null;
        }

        var cargo = r.Cargo?.Trim() ?? string.Empty;

        if (cargo.Length == 0)
        {
            erro = "O cargo é obrigatório.";
            return null;
        }

        if (cargo.Length > 100)
        {
            erro = "O cargo deve ter no máximo 100 caracteres.";
            return null;
        }

        if (r.Salario.HasValue && r.Salario.Value < 0)
        {
            erro = "O salário não pode ser negativo.";
            return null;
        }

        var statusInformado = r.Status?.Trim() ?? string.Empty;

        if (statusInformado.Length == 0)
        {
            erro = "O status é obrigatório.";
            return null;
        }

        string status;

        if (statusInformado.Equals(StatusFuncionario.Ativo, StringComparison.OrdinalIgnoreCase))
        {
            status = StatusFuncionario.Ativo;
        }
        else if (statusInformado.Equals(StatusFuncionario.Desligado, StringComparison.OrdinalIgnoreCase))
        {
            status = StatusFuncionario.Desligado;
        }
        else
        {
            erro = "Status inválido. Use Ativo ou Desligado.";
            return null;
        }

        if (status == StatusFuncionario.Desligado && !r.DataDesligamento.HasValue)
        {
            erro = "Informe a data de desligamento do funcionário.";
            return null;
        }

        var observacao = LimparOpcional(r.Observacao);

        if (observacao != null && observacao.Length > 500)
        {
            erro = "A observação deve ter no máximo 500 caracteres.";
            return null;
        }

        return new DadosValidados
        {
            Nome = nome,
            Cpf = cpf,
            Telefone = telefone,
            Cargo = cargo,
            Salario = r.Salario.HasValue ? Math.Round(r.Salario.Value, 2) : null,
            DataContratacao = r.DataContratacao?.Date,
            // Funcionário ativo não guarda data de desligamento.
            DataDesligamento = status == StatusFuncionario.Desligado ? r.DataDesligamento!.Value.Date : null,
            Status = status,
            Observacao = observacao
        };
    }

    private static void AplicarPessoa(Pessoa pessoa, DadosValidados d)
    {
        pessoa.Nome = d.Nome;
        pessoa.CpfCnpj = d.Cpf;
        pessoa.Telefone = d.Telefone;
        pessoa.CargoOuFuncao = d.Cargo;
    }

    private static void AplicarVinculo(Funcionario f, DadosValidados d)
    {
        f.Cargo = d.Cargo;
        f.Salario = d.Salario;
        f.DataContratacao = d.DataContratacao;
        f.DataDesligamento = d.DataDesligamento;
        f.Status = d.Status;
        f.Observacao = d.Observacao;
    }

    private static string? LimparOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static FuncionarioResponse ParaResponse(Funcionario f) =>
        new()
        {
            Id = f.Id,
            PessoaId = f.PessoaId,
            PropriedadeId = f.PropriedadeId,
            Nome = f.Pessoa?.Nome ?? string.Empty,
            Cpf = f.Pessoa?.CpfCnpj,
            Telefone = f.Pessoa?.Telefone,
            Cargo = f.Cargo,
            Salario = f.Salario,
            DataContratacao = f.DataContratacao,
            DataDesligamento = f.DataDesligamento,
            Status = f.Status,
            Observacao = f.Observacao
        };
}


using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class TransacaoFinanceiraService : ITransacaoFinanceiraService
{
    private readonly ITransacaoFinanceiraRepository _repository;
    private readonly IPropriedadeService _propriedadeService;

    public TransacaoFinanceiraService(
        ITransacaoFinanceiraRepository repository,
        IPropriedadeService propriedadeService)
    {
        _repository = repository;
        _propriedadeService = propriedadeService;
    }

    private async Task<bool> PropriedadeAutorizadaAsync(
        int propriedadeId, int usuarioId)
    {
        if (propriedadeId <= 0)
            return false;

        var resultado = await _propriedadeService
            .BuscarPorIdAsync(propriedadeId, usuarioId);

        return resultado.Success && resultado.Data is not null;
    }

    private async Task<TransacaoFinanceira?> BuscarAutorizadaAsync(
        int id, int usuarioId)
    {
        var transacao = await _repository.BuscarPorIdAsync(id);

        if (transacao is null)
            return null;

        return await PropriedadeAutorizadaAsync(
            transacao.PropriedadeId, usuarioId)
            ? transacao
            : null;
    }

    private async Task<List<TransacaoFinanceira>> ListarAutorizadasAsync(
        int usuarioId)
    {
        var resultado = await _propriedadeService
            .ListarPorUsuarioIdAsync(usuarioId);

        if (!resultado.Success || resultado.Data is null)
            throw new InvalidOperationException(
                "Não foi possível consultar as propriedades do usuário.");

        var transacoes = new List<TransacaoFinanceira>();

        foreach (var propriedade in resultado.Data)
        {
            var registros = await _repository
                .ListarPorPropriedadeIdAsync(propriedade.Id);

            transacoes.AddRange(registros);
        }

        return transacoes;
    }

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        ListarTodosAsync(int usuarioId)
    {
        var transacoes = await ListarAutorizadasAsync(usuarioId);

        return new(
            true,
            "Transações listadas com sucesso.",
            transacoes.Select(MapToResponse).ToList(),
            []);
    }

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        BuscarPorPeriodoAsync(
            DateTime inicio, DateTime fim, int usuarioId)
    {
        if (inicio > fim)
            return new(
                false,
                "Período inválido.",
                null,
                ["A data inicial deve ser anterior ou igual à final."]);

        var transacoes = await ListarAutorizadasAsync(usuarioId);

        var filtradas = transacoes
            .Where(t => t.DataVencimento >= inicio &&
                        t.DataVencimento <= fim)
            .Select(MapToResponse)
            .ToList();

        return new(
            true,
            "Transações do período listadas com sucesso.",
            filtradas,
            []);
    }

    public async Task<AppResponse<TransacaoFinanceiraResponse>>
        BuscarPorIdAsync(int id, int usuarioId)
    {
        var transacao = await BuscarAutorizadaAsync(id, usuarioId);

        if (transacao is null)
            return new(
                false,
                "Transação não encontrada.",
                null,
                ["Transação não encontrada."]);

        return new(
            true,
            "Transação recuperada com sucesso.",
            MapToResponse(transacao),
            []);
    }

    public async Task<AppResponse<TransacaoFinanceiraResponse>>
        InserirAsync(TransacaoFinanceiraRequest request, int usuarioId)
    {
        var erros = Validar(request);

        if (request.PropriedadeId <= 0)
            erros.Add("Informe uma propriedade válida.");

        if (erros.Count > 0)
            return new(false, "Dados inválidos.", null, erros);

        if (!await PropriedadeAutorizadaAsync(
                request.PropriedadeId, usuarioId))
            return new(
                false,
                "Propriedade não encontrada.",
                null,
                ["Propriedade não encontrada."]);

        var hoje = DateTime.Now.Date;
        var data = (request.Data ?? request.DataVencimento ?? hoje).Date;
        var vencimento = (request.DataVencimento ?? request.Data ?? hoje).Date;
        var status = request.Status ?? StatusTransacao.Pago;

        var transacao = new TransacaoFinanceira
        {
            PropriedadeId = request.PropriedadeId,
            Descricao = request.Descricao.Trim(),
            Valor = request.Valor,
            Tipo = request.Tipo,
            Categoria = string.IsNullOrWhiteSpace(request.Categoria)
                ? "Geral"
                : request.Categoria.Trim(),
            Data = data,
            DataVencimento = vencimento,
            DataPagamento = request.DataPagamento?.Date
                ?? (status == StatusTransacao.Pago ? data : null),
            Status = status,
            SafraId = request.SafraId,
            AnimalId = request.AnimalId
        };

        var criada = await _repository.InserirAsync(transacao);

        return new(
            true,
            "Transação criada com sucesso.",
            MapToResponse(criada),
            []);
    }

    public async Task<AppResponse<bool>> AtualizarAsync(
        int id, TransacaoFinanceiraRequest request, int usuarioId)
    {
        var transacao = await BuscarAutorizadaAsync(id, usuarioId);

        if (transacao is null)
            return new(
                false,
                "Transação não encontrada.",
                false,
                ["Transação não encontrada."]);

        var erros = Validar(request);

        if (request.PropriedadeId < 0)
            erros.Add("PropriedadeId inválido.");

        if (request.PropriedadeId > 0 &&
            request.PropriedadeId != transacao.PropriedadeId)
            erros.Add(
                "Não é permitido alterar a propriedade de um lançamento.");

        if (erros.Count > 0)
            return new(false, "Dados inválidos.", false, erros);

        var statusAnterior = transacao.Status;

        transacao.Descricao = request.Descricao.Trim();
        transacao.Valor = request.Valor;
        transacao.Tipo = request.Tipo;

        if (!string.IsNullOrWhiteSpace(request.Categoria))
            transacao.Categoria = request.Categoria.Trim();

        if (request.Data.HasValue)
            transacao.Data = request.Data.Value.Date;

        if (request.DataVencimento.HasValue)
            transacao.DataVencimento = request.DataVencimento.Value.Date;

        if (request.SafraId.HasValue)
            transacao.SafraId = request.SafraId;

        if (request.AnimalId.HasValue)
            transacao.AnimalId = request.AnimalId;

        if (request.Status.HasValue)
            transacao.Status = request.Status.Value;

        if (request.DataPagamento.HasValue)
        {
            transacao.DataPagamento = request.DataPagamento.Value.Date;
        }
        else if (statusAnterior != StatusTransacao.Pago &&
                 transacao.Status == StatusTransacao.Pago)
        {
            transacao.DataPagamento = DateTime.Now.Date;
        }

        await _repository.AtualizarAsync(transacao);

        return new(
            true,
            "Transação atualizada com sucesso.",
            true,
            []);
    }

    public async Task<AppResponse<bool>> ExcluirAsync(int id, int usuarioId)
    {
        var transacao = await BuscarAutorizadaAsync(id, usuarioId);

        if (transacao is null)
            return new(
                false,
                "Transação não encontrada.",
                false,
                ["Transação não encontrada."]);

        await _repository.ExcluirAsync(id);

        return new(
            true,
            "Transação excluída com sucesso.",
            true,
            []);
    }

    public async Task<AppResponse<ResumoFinanceiroResponse>>
        ObterResumoFinanceiroAsync(int usuarioId)
    {
        var transacoes = await ListarAutorizadasAsync(usuarioId);

        var pagas = transacoes
            .Where(t => t.Status == StatusTransacao.Pago)
            .ToList();

        var receitas = pagas
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        var despesas = pagas
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);

        var resumo = new ResumoFinanceiroResponse(
            receitas, despesas, receitas - despesas);

        return new(
            true,
            "Resumo financeiro calculado com sucesso.",
            resumo,
            []);
    }

    private static List<string> Validar(
        TransacaoFinanceiraRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Descricao))
            erros.Add("A descrição é obrigatória.");

        if (request.Valor <= 0)
            erros.Add("O valor deve ser maior que zero.");

        if (!Enum.IsDefined(request.Tipo))
            erros.Add("Tipo de transação inválido.");

        if (request.Status.HasValue &&
            !Enum.IsDefined(request.Status.Value))
            erros.Add("Status da transação inválido.");

        if (request.SafraId is <= 0)
            erros.Add("SafraId deve ser positivo.");

        if (request.AnimalId is <= 0)
            erros.Add("AnimalId deve ser positivo.");

        return erros;
    }

    private static TransacaoFinanceiraResponse MapToResponse(
        TransacaoFinanceira t) =>
        new(
            t.Id,
            t.Descricao,
            t.Valor,
            t.Tipo.ToString(),
            t.Categoria,
            t.DataVencimento,
            t.DataPagamento,
            t.Status.ToString(),
            t.AnimalId,
            t.PropriedadeId,
            t.Data,
            t.SafraId
        );
}

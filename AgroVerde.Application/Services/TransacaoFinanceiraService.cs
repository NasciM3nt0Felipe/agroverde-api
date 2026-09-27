using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class TransacaoFinanceiraService : ITransacaoFinanceiraService
{
    private readonly ITransacaoFinanceiraRepository _repository;

    public TransacaoFinanceiraService(ITransacaoFinanceiraRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>> ListarTodosAsync()
    {
        var transacoes = await _repository.ListarTodosAsync();
        return new AppResponse<IEnumerable<TransacaoFinanceiraResponse>>(true, "Transações listadas com sucesso.", transacoes.Select(MapToResponse), []);
    }

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>> BuscarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var transacoes = await _repository.BuscarPorPeriodoAsync(inicio, fim);
        return new AppResponse<IEnumerable<TransacaoFinanceiraResponse>>(true, "Transações do período listadas com sucesso.", transacoes.Select(MapToResponse), []);
    }

    public async Task<AppResponse<TransacaoFinanceiraResponse>> BuscarPorIdAsync(int id)
    {
        var transacao = await _repository.BuscarPorIdAsync(id);
        if (transacao is null)
            return new AppResponse<TransacaoFinanceiraResponse>(false, "Transação não encontrada.", null, ["Transação não encontrada."]);

        return new AppResponse<TransacaoFinanceiraResponse>(true, "Transação recuperada com sucesso.", MapToResponse(transacao), []);
    }

    public async Task<AppResponse<TransacaoFinanceiraResponse>> InserirAsync(TransacaoFinanceiraRequest request)
    {
        var transacao = new TransacaoFinanceira
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            Tipo = request.Tipo,
            Categoria = request.Categoria,
            DataVencimento = request.DataVencimento,
            DataPagamento = request.DataPagamento,
            Status = request.Status,
            AnimalId = request.AnimalId
        };

        var criada = await _repository.InserirAsync(transacao);
        return new AppResponse<TransacaoFinanceiraResponse>(true, "Transação criada com sucesso.", MapToResponse(criada), []);
    }

    public async Task<AppResponse<bool>> AtualizarAsync(int id, TransacaoFinanceiraRequest request)
    {
        var transacao = await _repository.BuscarPorIdAsync(id);
        if (transacao is null)
            return new AppResponse<bool>(false, "Transação não encontrada.", false, ["Transação não encontrada."]);

        transacao.Descricao = request.Descricao;
        transacao.Valor = request.Valor;
        transacao.Tipo = request.Tipo;
        transacao.Categoria = request.Categoria;
        transacao.DataVencimento = request.DataVencimento;
        transacao.DataPagamento = request.DataPagamento;
        transacao.Status = request.Status;
        transacao.AnimalId = request.AnimalId;

        await _repository.AtualizarAsync(transacao);
        return new AppResponse<bool>(true, "Transação atualizada com sucesso.", true, []);
    }

    public async Task<AppResponse<bool>> ExcluirAsync(int id)
    {
        var transacao = await _repository.BuscarPorIdAsync(id);
        if (transacao is null)
            return new AppResponse<bool>(false, "Transação não encontrada.", false, ["Transação não encontrada."]);

        await _repository.ExcluirAsync(id);
        return new AppResponse<bool>(true, "Transação excluída com sucesso.", true, []);
    }

    public async Task<AppResponse<ResumoFinanceiroResponse>> ObterResumoFinanceiroAsync()
    {
        var transacoes = await _repository.ListarTodosAsync();
        var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita && t.Status == StatusTransacao.Pago).Sum(t => t.Valor);
        var despesas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa && t.Status == StatusTransacao.Pago).Sum(t => t.Valor);

        var resumo = new ResumoFinanceiroResponse(receitas, despesas, receitas - despesas);
        return new AppResponse<ResumoFinanceiroResponse>(true, "Resumo financeiro calculado com sucesso.", resumo, []);
    }

    private static TransacaoFinanceiraResponse MapToResponse(TransacaoFinanceira t) =>
        new(t.Id, t.Descricao, t.Valor, t.Tipo.ToString(), t.Categoria, t.DataVencimento, t.DataPagamento, t.Status.ToString(), t.AnimalId);
}
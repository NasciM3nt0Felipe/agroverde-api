
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class TransacaoFinanceiraService : ITransacaoFinanceiraService
{
    private readonly ITransacaoFinanceiraRepository _repository;
    private readonly IPropriedadeService _propriedadeService;
    private readonly ISafraRepository _safraRepository;
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IAnimalRepository _animalRepository;

    public TransacaoFinanceiraService(
        ITransacaoFinanceiraRepository repository,
        IPropriedadeService propriedadeService,
        ISafraRepository safraRepository,
        ITalhaoRepository talhaoRepository,
        IAnimalRepository animalRepository)
    {
        _repository = repository;
        _propriedadeService = propriedadeService;
        _safraRepository = safraRepository;
        _talhaoRepository = talhaoRepository;
        _animalRepository = animalRepository;
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

    // Retorna null quando a propriedade filtrada não pertence ao usuário.
    private async Task<List<TransacaoFinanceira>?> ListarAutorizadasAsync(
        int usuarioId, int? propriedadeId = null)
    {
        if (propriedadeId.HasValue)
        {
            if (!await PropriedadeAutorizadaAsync(
                    propriedadeId.Value, usuarioId))
                return null;

            return (await _repository
                    .ListarPorPropriedadeIdAsync(propriedadeId.Value))
                .ToList();
        }

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

        return transacoes
            .OrderByDescending(t => t.Id)
            .ToList();
    }

    private static AppResponse<T> PropriedadeNaoEncontrada<T>() =>
        new(
            false,
            ITransacaoFinanceiraService.MensagemPropriedadeNaoEncontrada,
            default,
            [ITransacaoFinanceiraService.MensagemPropriedadeNaoEncontrada]);

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        ListarTodosAsync(int usuarioId, int? propriedadeId = null)
    {
        var transacoes = await ListarAutorizadasAsync(
            usuarioId, propriedadeId);

        if (transacoes is null)
            return PropriedadeNaoEncontrada<
                IEnumerable<TransacaoFinanceiraResponse>>();

        return new(
            true,
            "Transações listadas com sucesso.",
            transacoes.Select(MapToResponse).ToList(),
            []);
    }

    public async Task<AppResponse<IEnumerable<TransacaoFinanceiraResponse>>>
        BuscarPorPeriodoAsync(
            DateTime inicio,
            DateTime fim,
            int usuarioId,
            int? propriedadeId = null)
    {
        if (inicio > fim)
            return new(
                false,
                "Período inválido.",
                null,
                ["A data inicial deve ser anterior ou igual à final."]);

        var transacoes = await ListarAutorizadasAsync(
            usuarioId, propriedadeId);

        if (transacoes is null)
            return PropriedadeNaoEncontrada<
                IEnumerable<TransacaoFinanceiraResponse>>();

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
                ITransacaoFinanceiraService.MensagemNaoEncontrada,
                null,
                [ITransacaoFinanceiraService.MensagemNaoEncontrada]);

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

        var status = request.Status ?? StatusTransacao.Pago;

        // Só lançamentos pagos possuem data de pagamento.
        if (status != StatusTransacao.Pago && request.DataPagamento.HasValue)
            erros.Add(
                "A data de pagamento só pode ser informada para lançamentos pagos.");

        // Origem é opcional; quando informada, precisa ser uma origem do app.
        // A origem "Estoque" é reservada aos lançamentos gerados pela API.
        var origem = OrigemFinanceira.NormalizarOrigemDoApp(request.OrigemTipo);

        if (!string.IsNullOrWhiteSpace(request.OrigemTipo) && origem is null)
            erros.Add("Origem do lançamento inválida.");

        if (erros.Count > 0)
            return new(false, "Dados inválidos.", null, erros);

        if (!await PropriedadeAutorizadaAsync(
                request.PropriedadeId, usuarioId))
            return PropriedadeNaoEncontrada<TransacaoFinanceiraResponse>();

        // Safra e animal precisam pertencer à mesma propriedade.
        erros.AddRange(await ValidarVinculosAsync(
            request.PropriedadeId, request.SafraId, request.AnimalId));

        if (erros.Count > 0)
            return new(false, "Dados inválidos.", null, erros);

        var hoje = DateTime.Now.Date;
        var data = (request.Data ?? request.DataVencimento ?? hoje).Date;
        var vencimento = (request.DataVencimento ?? request.Data ?? hoje).Date;

        // A mesma folha (mesma descrição e data) não pode ser lançada duas
        // vezes na propriedade, exceto se a anterior tiver sido cancelada.
        if (origem == OrigemFinanceira.FolhaPagamento)
        {
            var descricao = request.Descricao.Trim();

            var existentes = await _repository
                .ListarPorPropriedadeIdAsync(request.PropriedadeId);

            var jaLancada = existentes.Any(t =>
                t.OrigemTipo == OrigemFinanceira.FolhaPagamento &&
                t.Status != StatusTransacao.Cancelado &&
                t.Data.Date == data &&
                string.Equals(t.Descricao, descricao, StringComparison.Ordinal));

            if (jaLancada)
            {
                const string mensagem =
                    "A folha deste funcionário já foi lançada nesta data.";
                return new(false, mensagem, null, [mensagem]);
            }
        }

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
            DataPagamento = status == StatusTransacao.Pago
                ? (request.DataPagamento?.Date ?? data)
                : null,
            Status = status,
            SafraId = request.SafraId,
            AnimalId = request.AnimalId,
            // OrigemId fica nulo: o ID local do app não é único entre
            // usuários e colidiria no índice único (OrigemTipo, OrigemId).
            OrigemTipo = origem
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
                ITransacaoFinanceiraService.MensagemNaoEncontrada,
                false,
                [ITransacaoFinanceiraService.MensagemNaoEncontrada]);

        // Lançamentos automáticos têm os valores definidos pelo módulo
        // de origem e não podem ser editados pelo Financeiro.
        if (!OrigemFinanceira.PermiteEditar(transacao))
        {
            var mensagem = MensagemNaoEditavel(transacao.OrigemTipo);
            return new(false, mensagem, false, [mensagem]);
        }

        var erros = Validar(request);

        if (request.PropriedadeId < 0)
            erros.Add("PropriedadeId inválido.");

        if (request.PropriedadeId > 0 &&
            request.PropriedadeId != transacao.PropriedadeId)
            erros.Add(
                "Não é permitido alterar a propriedade de um lançamento.");

        var statusFinal = request.Status ?? transacao.Status;

        if (statusFinal != StatusTransacao.Pago &&
            request.DataPagamento.HasValue)
            erros.Add(
                "A data de pagamento só pode ser informada para lançamentos pagos.");

        if (erros.Count > 0)
            return new(false, "Dados inválidos.", false, erros);

        // Valida apenas os vínculos que estão sendo alterados.
        erros.AddRange(await ValidarVinculosAsync(
            transacao.PropriedadeId,
            request.SafraId.HasValue &&
                request.SafraId != transacao.SafraId
                ? request.SafraId
                : null,
            request.AnimalId.HasValue &&
                request.AnimalId != transacao.AnimalId
                ? request.AnimalId
                : null));

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

        transacao.Status = statusFinal;

        // Mantém DataPagamento coerente com o status.
        if (transacao.Status != StatusTransacao.Pago)
        {
            transacao.DataPagamento = null;
        }
        else if (request.DataPagamento.HasValue)
        {
            transacao.DataPagamento = request.DataPagamento.Value.Date;
        }
        else if (statusAnterior != StatusTransacao.Pago ||
                 transacao.DataPagamento is null)
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
                ITransacaoFinanceiraService.MensagemNaoEncontrada,
                false,
                [ITransacaoFinanceiraService.MensagemNaoEncontrada]);

        // Lançamentos gerados pela API (ex.: compra em /api/Estoque)
        // pertencem ao módulo de origem e não podem ser excluídos aqui.
        if (!OrigemFinanceira.PermiteExcluir(transacao))
        {
            var mensagem = MensagemNaoExcluivel(transacao.OrigemTipo);
            return new(false, mensagem, false, [mensagem]);
        }

        await _repository.ExcluirAsync(id);

        return new(
            true,
            "Transação excluída com sucesso.",
            true,
            []);
    }

    public async Task<AppResponse<ResumoFinanceiroResponse>>
        ObterResumoFinanceiroAsync(int usuarioId, int? propriedadeId = null)
    {
        var transacoes = await ListarAutorizadasAsync(
            usuarioId, propriedadeId);

        if (transacoes is null)
            return PropriedadeNaoEncontrada<ResumoFinanceiroResponse>();

        // Somente lançamentos pagos compõem o saldo disponível:
        // pendentes e cancelados não aumentam nem reduzem o saldo.
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

    // Garante que safra e animal pertencem à propriedade do lançamento.
    private async Task<List<string>> ValidarVinculosAsync(
        int propriedadeId, int? safraId, int? animalId)
    {
        var erros = new List<string>();

        if (safraId is > 0)
        {
            var safra = await _safraRepository
                .BuscarPorIdAsync(safraId.Value);

            var talhao = safra is null
                ? null
                : await _talhaoRepository.BuscarPorIdAsync(safra.TalhaoId);

            if (talhao is null || talhao.PropriedadeId != propriedadeId)
                erros.Add("Safra não encontrada nesta propriedade.");
        }

        if (animalId is > 0)
        {
            var animal = await _animalRepository
                .BuscarPorIdAsync(animalId.Value);

            if (animal is null || animal.PropriedadeId != propriedadeId)
                erros.Add("Animal não encontrado nesta propriedade.");
        }

        return erros;
    }

    private static string MensagemNaoEditavel(string? origem) =>
        $"Lançamento automático ({origem}): os valores são definidos pelo " +
        "módulo de origem e não podem ser editados pelo Financeiro.";

    private static string MensagemNaoExcluivel(string? origem) =>
        $"Lançamento automático ({origem}): pertence ao módulo de origem " +
        "e não pode ser excluído pelo Financeiro.";

    private static List<string> Validar(
        TransacaoFinanceiraRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Descricao))
            erros.Add("A descrição é obrigatória.");
        else if (request.Descricao.Trim().Length > 200)
            erros.Add("A descrição deve ter no máximo 200 caracteres.");

        if (request.Categoria is { Length: > 0 } &&
            request.Categoria.Trim().Length > 100)
            erros.Add("A categoria deve ter no máximo 100 caracteres.");

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
            t.SafraId,
            t.OrigemTipo,
            OrigemFinanceira.PermiteEditar(t),
            OrigemFinanceira.PermiteExcluir(t)
        );
}

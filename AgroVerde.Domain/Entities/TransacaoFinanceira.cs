namespace AgroVerde.Domain.Entities;

public enum TipoTransacao
{
    Receita = 1,
    Despesa = 2
}

public enum StatusTransacao
{
    Pendente = 1,
    Pago = 2,
    Cancelado = 3
}

public class TransacaoFinanceira
{
    public int Id { get; set; }

    // Vincula o lançamento à propriedade em foco.
    public int PropriedadeId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public TipoTransacao Tipo { get; set; }

    // Data principal usada pelo Financeiro do Flutter.
    public DateTime Data { get; set; }

    // Safra é opcional no Flutter.
    public int? SafraId { get; set; }

    // Campos adicionais implementados pelo Maicon.
    public string Categoria { get; set; } = string.Empty;

    public DateTime DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    public StatusTransacao Status { get; set; } = StatusTransacao.Pendente;

    public int? AnimalId { get; set; }

    public Animal? Animal { get; set; }
    public string? OrigemTipo { get; set; }
    public int? OrigemId { get; set; }
}

public static class OrigemFinanceira
{
    // Origem gerada pela própria API (ex.: compra registrada em /api/Estoque).
    // O lançamento é mantido pelo módulo de origem: não pode ser alterado
    // nem excluído pelo Financeiro.
    public const string Estoque = "Estoque";

    // Também geradas pela API (/api/Veiculo): a despesa é mantida pelo
    // módulo Veículos (alterar ou excluir o abastecimento/manutenção).
    public const string Abastecimento = "Abastecimento";
    public const string ManutencaoVeiculo = "ManutencaoVeiculo";

    // Origens informadas pelo app para módulos que ainda gravam localmente
    // (SQLite). O lançamento não pode ser editado pelo Financeiro (os valores
    // vêm do módulo de origem), mas pode ser excluído para corrigir enganos.
    public const string CompraEstoque = "CompraEstoque";
    public const string FolhaPagamento = "FolhaPagamento";
    public const string VendaGraos = "VendaGraos";
    public const string VendaVeiculo = "VendaVeiculo";

    private static readonly string[] InformadasPeloApp =
    [
        CompraEstoque,
        FolhaPagamento,
        VendaGraos,
        VendaVeiculo
    ];

    // Retorna o nome canônico de uma origem aceita do app, ou null.
    public static string? NormalizarOrigemDoApp(string? origem)
    {
        if (string.IsNullOrWhiteSpace(origem))
            return null;

        return InformadasPeloApp.FirstOrDefault(o =>
            string.Equals(o, origem.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool PermiteEditar(TransacaoFinanceira t) =>
        string.IsNullOrWhiteSpace(t.OrigemTipo);

    public static bool PermiteExcluir(TransacaoFinanceira t) =>
        string.IsNullOrWhiteSpace(t.OrigemTipo) ||
        NormalizarOrigemDoApp(t.OrigemTipo) is not null;
}
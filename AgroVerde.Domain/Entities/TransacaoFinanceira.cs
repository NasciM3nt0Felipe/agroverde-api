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
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusTransacao Status { get; set; } = StatusTransacao.Pendente;

    public int? AnimalId { get; set; }
    public Animal? Animal { get; set; }
}
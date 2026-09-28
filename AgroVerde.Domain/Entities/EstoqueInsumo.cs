namespace AgroVerde.Domain.Entities;

public class EstoqueInsumo
{
    public int Id { get; set; }
    public int SafraId { get; set; }
    public int EstoqueItemId { get; set; }
    public double QuantidadeUtilizada { get; set; }
    public double ValorTotal { get; set; }
    public string DataMovimentacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}